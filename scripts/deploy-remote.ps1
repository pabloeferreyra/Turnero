Param(
    [Parameter(Mandatory = $true)]
    [Alias("Host")]
    [string]$RemoteHost,

    [Parameter(Mandatory = $true)]
    [string]$User,

    [Parameter(Mandatory = $false)]
    [string]$Version = $(
        if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_RUN_NUMBER)) {
            "run-$($env:GITHUB_RUN_NUMBER)"
        }
        elseif (-not [string]::IsNullOrWhiteSpace($env:GITHUB_RUN_ID)) {
            "runid-$($env:GITHUB_RUN_ID)"
        }
        else {
            throw "Version is required when not running in GitHub Actions. Pass -Version or set GITHUB_RUN_NUMBER."
        }
    ),

    [string]$RemotePath = "/opt/turnero",
    [string]$StackName = "turnero",
    [string]$StackFilePath = "docker-compose.prod.yml",
    [switch]$UseTls,
    [string]$ImageRepo = "turnero-app",
    [string]$FirebaseCredentialsFile = "/opt/secrets/firebase.json",
    [string]$SshKeyPath,
    [int]$SshPort = $(
        if (-not [string]::IsNullOrWhiteSpace($env:DEPLOY_SSH_PORT)) {
            [int]$env:DEPLOY_SSH_PORT
        }
        elseif (-not [string]::IsNullOrWhiteSpace($env:SSH_PORT)) {
            [int]$env:SSH_PORT
        }
        else {
            22
        }
    ),
    [switch]$SyncEnv,
    [string]$EnvFilePath = ".env",
    [switch]$BackupEnv,
    [int]$BackupEnvRetention = 10
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not (Get-Command ssh -ErrorAction SilentlyContinue)) {
    throw "ssh command not found. Install OpenSSH Client in Windows."
}

if (-not (Get-Command scp -ErrorAction SilentlyContinue)) {
    throw "scp command not found. Install OpenSSH Client in Windows."
}

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "docker command not found. Install Docker and ensure it is available in PATH."
}

$null = (& docker version --format '{{.Server.Version}}' 2>$null)
if ($LASTEXITCODE -ne 0) {
    throw "Docker daemon is not available locally. Start Docker Desktop (or Docker Engine) and retry."
}

if ($BackupEnvRetention -lt 0) {
    throw "BackupEnvRetention cannot be negative. Use 0 to disable pruning, or a positive number to keep that many backups."
}

if ($UseTls -and -not $PSBoundParameters.ContainsKey("StackFilePath")) {
    $StackFilePath = "docker-compose.prod.yml"
}

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

function Resolve-LocalPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (Test-Path $Path) {
        return (Resolve-Path $Path).Path
    }

    $fromProjectRoot = Join-Path $projectRoot $Path
    if (Test-Path $fromProjectRoot) {
        return (Resolve-Path $fromProjectRoot).Path
    }

    throw "Path not found: $Path"
}

$localComposeFullPath = Resolve-LocalPath -Path $StackFilePath
$composeFileName = Split-Path -Leaf $localComposeFullPath
$imageTag = "$ImageRepo`:$Version"

$sshArgs = @()
$scpArgs = @()
if (-not [string]::IsNullOrWhiteSpace($SshKeyPath)) {
    $sshArgs += @("-i", $SshKeyPath)
    $scpArgs += @("-i", $SshKeyPath)
}
$sshArgs += @("-p", $SshPort)
$scpArgs += @("-P", $SshPort)

$target = "$User@$RemoteHost"

function Invoke-RemoteBash {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Script,
        [switch]$SilenceStdErr
    )

    # Ensure Linux shell receives LF-only script content.
    $normalizedScript = $Script -replace "`r", ""
    $encodedScript = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($normalizedScript))
    $remoteCommand = "echo '$encodedScript' | base64 -d | bash"

    if ($SilenceStdErr) {
        return (& ssh @sshArgs $target $remoteCommand 2>$null)
    }

    return (& ssh @sshArgs $target $remoteCommand)
}

function Get-RemoteFileHash {
    param(
        [string]$RemoteFilePath
    )

    $remoteHashScript = @"
if [ -f '$RemoteFilePath' ]; then
  if command -v sha256sum >/dev/null 2>&1; then
        sha256sum '$RemoteFilePath' | awk '{print `$1}'
  elif command -v shasum >/dev/null 2>&1; then
        shasum -a 256 '$RemoteFilePath' | awk '{print `$1}'
  fi
fi
"@

    $remoteHash = (Invoke-RemoteBash -Script $remoteHashScript -SilenceStdErr | Select-Object -First 1)
    return ($remoteHash ?? "").Trim()
}

function Backup-RemoteEnvFile {
    param(
        [string]$RemoteFilePath
    )

    $remoteBackupScript = @"
if [ -f '$RemoteFilePath' ]; then
    backup_path="${RemoteFilePath}.bak.`$(date +%Y%m%d%H%M%S)"
    cp '$RemoteFilePath' "`$backup_path"
    echo "`$backup_path"
fi
"@

    $backupPath = (Invoke-RemoteBash -Script $remoteBackupScript -SilenceStdErr | Select-Object -First 1)
    return ($backupPath ?? "").Trim()
}

function Prune-RemoteEnvBackups {
    param(
        [string]$RemoteFilePath,
        [int]$KeepCount
    )

    if ($KeepCount -eq 0) {
        return "-1"
    }

    $pruneStart = $KeepCount + 1
    $remotePruneScript = @"
pattern='${RemoteFilePath}.bak.*'
if ls -1 "`$pattern" >/dev/null 2>&1; then
  ls -1t "`$pattern" | tail -n +$pruneStart | xargs -r rm -f --
fi
remaining=`$(ls -1 "`$pattern" 2>/dev/null | wc -l | tr -d ' ')
echo "`$remaining"
"@

    $remaining = (Invoke-RemoteBash -Script $remotePruneScript -SilenceStdErr | Select-Object -First 1)
    return ($remaining ?? "").Trim()
}

Write-Host "Ensuring remote deploy directory exists: $RemotePath"
Invoke-RemoteBash -Script "mkdir -p '$RemotePath'" | Out-Null

if ($SyncEnv) {
    $localEnvFullPath = Resolve-LocalPath -Path $EnvFilePath

    if (-not (Test-Path $localEnvFullPath)) {
        throw "Local env file not found: $EnvFilePath"
    }

    $remoteEnvPath = "$RemotePath/.env"

    $localHash = (Get-FileHash -Path $localEnvFullPath -Algorithm SHA256).Hash.ToLowerInvariant()
    $remoteHash = (Get-RemoteFileHash -RemoteFilePath $remoteEnvPath).ToLowerInvariant()

    if ($localHash -ne $remoteHash) {
        if ($BackupEnv) {
            $backupPath = Backup-RemoteEnvFile -RemoteFilePath $remoteEnvPath
            if (-not [string]::IsNullOrWhiteSpace($backupPath)) {
                Write-Host "Remote .env backup created: $backupPath"

                $remainingBackups = Prune-RemoteEnvBackups -RemoteFilePath $remoteEnvPath -KeepCount $BackupEnvRetention
                if ($remainingBackups -eq "-1") {
                    Write-Host "Backup retention disabled (BackupEnvRetention=0)."
                }
                elseif (-not [string]::IsNullOrWhiteSpace($remainingBackups)) {
                    Write-Host "Remote .env backups retained: $remainingBackups (max $BackupEnvRetention)."
                }
            }
            else {
                Write-Host "No remote .env found to back up; continuing with sync."
            }
        }

        Write-Host "Syncing .env to ${target}:$remoteEnvPath"
        & scp @scpArgs $localEnvFullPath "${target}:$remoteEnvPath"
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to upload .env to remote path: $remoteEnvPath"
        }
    }
    else {
        Write-Host ".env unchanged, skipping sync."
    }
}

Write-Host "Building image locally: $imageTag"
Push-Location $projectRoot
try {
    & docker build -t $imageTag .
}
finally {
    Pop-Location
}

Write-Host "Syncing compose file to ${target}:$RemotePath/$composeFileName"
& scp @scpArgs $localComposeFullPath "${target}:$RemotePath/$composeFileName"
if ($LASTEXITCODE -ne 0) {
    throw "Failed to upload compose file to remote path: $RemotePath/$composeFileName"
}

Write-Host "Transferring Docker image to remote host: $imageTag"
$tempImageTar = Join-Path ([System.IO.Path]::GetTempPath()) ("$($ImageRepo)-$($Version)-" + [guid]::NewGuid().ToString("N") + ".tar")
$remoteImageTar = "$RemotePath/.image-$Version.tar"

try {
    & docker save -o $tempImageTar $imageTag
    if ($LASTEXITCODE -ne 0) {
        throw "docker save failed for image $imageTag"
    }

    & scp @scpArgs $tempImageTar "${target}:$remoteImageTar"
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to upload image tar to remote host"
    }

    # Load image on remote and tag for compose
    Invoke-RemoteBash -Script @"
set -e
echo '   Loading image on remote...'
docker load -i '$remoteImageTar'
echo '   Tagging as $ImageRepo:prod for docker compose...'
docker tag '$imageTag' '$ImageRepo`:prod'
rm -f '$remoteImageTar'
echo '   Image loaded and tagged successfully.'
"@ | Out-Host
}
finally {
    if (Test-Path $tempImageTar) {
        Remove-Item -Path $tempImageTar -Force -ErrorAction SilentlyContinue
    }
}

# ── Deploy with docker compose ─────────────────────────────
$remoteScript = @"
set -e
cd '$RemotePath'

if [ ! -f .env ]; then
    echo "Warning: .env file not found in $RemotePath." >&2
    echo "The app may not start correctly without environment variables." >&2
elif ! grep -q '^ConnectionStrings__PostgresConnection=' .env; then
    echo "Warning: .env is missing ConnectionStrings__PostgresConnection." >&2
    echo "The app will not be able to connect to the database." >&2
fi

# Export variables needed by docker compose for interpolation
export FIREBASE_CREDENTIALS_FILE='$FirebaseCredentialsFile'

echo '   Stopping existing containers (if any)...'
docker compose -f '$composeFileName' down --remove-orphans 2>/dev/null || true

# ── Retry loop for docker compose up ───────────────────────
RETRIES=3
for i in `$(seq 1 `$RETRIES); do
  echo "   [Attempt `$i/`$RETRIES] Running: docker compose -f $composeFileName up -d..."
  if docker compose -f '$composeFileName' up -d; then
    echo "   [Attempt `$i/`$RETRIES] Deploy succeeded."
    break
  fi
  if [ "`$i" -lt "`$RETRIES" ]; then
    echo "   [Attempt `$i/`$RETRIES] Failed. Retrying in 10s..."
    sleep 10
  else
    echo "   [Attempt `$i/`$RETRIES] All attempts failed." >&2
    exit 1
  fi
done

echo ''
echo '   Container status:'
docker ps --filter 'name=$StackName' --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'

echo ''
echo '   Compose services:'
docker compose -f '$composeFileName' ps

# ── Log deploy ──
log_file="$RemotePath/deploy.log"
timestamp=`$(date '+%Y-%m-%d %H:%M:%S')
log_entry="[`$timestamp] INFO: Deploy successful | version=$Version | image=$imageTag | compose=$composeFileName | user=$User"
echo "`$log_entry" >> "`$log_file"
echo "   Deploy logged to: `$log_file"
"@

Write-Host "Executing deploy on $target ($RemotePath) with version $Version..."
Invoke-RemoteBash -Script $remoteScript | Out-Host

Write-Host "Remote deploy command completed."
