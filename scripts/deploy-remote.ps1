Param(
    [Parameter(Mandatory = $true)]
    [Alias("Host")]
    [string]$RemoteHost,

    [Parameter(Mandatory = $true)]
    [string]$User,

    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$RemotePath = "/opt/turnero",
    [string]$StackName = "turnero",
    [string]$StackFilePath = "docker-stack.prod.yml",
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
    $StackFilePath = "docker-stack.tls.yml"
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

$localStackFullPath = Resolve-LocalPath -Path $StackFilePath
$stackFileName = Split-Path -Leaf $localStackFullPath
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

Write-Host "Syncing stack file to ${target}:$RemotePath/$stackFileName"
& scp @scpArgs $localStackFullPath "${target}:$RemotePath/$stackFileName"
if ($LASTEXITCODE -ne 0) {
    throw "Failed to upload stack file to remote path: $RemotePath/$stackFileName"
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

    Invoke-RemoteBash -Script "docker load -i '$remoteImageTar'; rm -f '$remoteImageTar'" | Out-Host
}
finally {
    if (Test-Path $tempImageTar) {
        Remove-Item -Path $tempImageTar -Force -ErrorAction SilentlyContinue
    }
}

$remoteScript = @"
set -eu
(set -o pipefail) 2>/dev/null && set -o pipefail
mkdir -p '$RemotePath'
cd '$RemotePath'
if [ ! -f .env ]; then
    echo "Error: .env file not found in $RemotePath. Use -SyncEnv or create it manually." >&2
    exit 1
fi

if ! grep -q '^ConnectionStrings__PostgresConnection=' .env; then
    echo "Error: .env is missing ConnectionStrings__PostgresConnection." >&2
    exit 1
fi

# Load .env values into current shell for docker stack variable interpolation.
while IFS= read -r line || [ -n "`$line" ]; do
    line=`$(printf '%s' "`$line" | tr -d '\r')
    case "`$line" in
        ''|\#*) continue ;;
    esac
    if [[ "`$line" == *=* ]]; then
        key="`${line%%=*}"
        val="`${line#*=}"
        key=`$(printf '%s' "`$key" | tr -d '\r')
        val=`$(printf '%s' "`$val" | tr -d '\r')
        export "`$key=`$val"
    fi
done < .env

if [ -z "`${ConnectionStrings__PostgresConnection:-}" ]; then
    echo "Error: ConnectionStrings__PostgresConnection resolved empty after loading .env." >&2
    exit 1
fi

if [ '$stackFileName' = 'docker-stack.tls.yml' ]; then
    tls_domain=`$(printf '%s' "`${LETSENCRYPT_DOMAIN:-}" | tr -d '\r' | sed -e 's/^"//' -e 's/"`$//' -e 's/^[[:space:]]*//' -e 's/[[:space:]]*`$//')

    if [ -z "`$tls_domain" ]; then
        echo "Error: LETSENCRYPT_DOMAIN is required for TLS deploy (docker-stack.tls.yml)." >&2
        exit 1
    fi

    cert_dir="/etc/letsencrypt/live/`$tls_domain"
    if [ ! -e "`$cert_dir" ]; then
        echo "Error: Let's Encrypt directory not found on remote host: `$cert_dir" >&2
        available_dirs=`$(ls -1 /etc/letsencrypt/live 2>/dev/null | tr '\n' ' ' || true)
        if [ -n "`$available_dirs" ]; then
            echo "Available /etc/letsencrypt/live entries: `$available_dirs" >&2
        fi
        exit 1
    fi

    if [ ! -f "`$cert_dir/fullchain.pem" ] || [ ! -f "`$cert_dir/privkey.pem" ]; then
        echo "Error: Missing certificate files in `$cert_dir (fullchain.pem and/or privkey.pem)." >&2
        exit 1
    fi
fi

if [ "`$(docker info --format '{{.Swarm.ControlAvailable}}' 2>/dev/null || echo false)" != "true" ]; then
    echo "Error: this node is not a Swarm manager. Run on a manager node or initialize swarm (docker swarm init)." >&2
    exit 1
fi
export TURNERO_IMAGE='$imageTag'
export FIREBASE_CREDENTIALS_FILE='$FirebaseCredentialsFile'
export REMOTE_DEPLOY_PATH='$RemotePath'
docker stack deploy -c '$stackFileName' '$StackName'
service_name='${StackName}_turnero-app'
docker service ls | grep "`$service_name" || true
docker service ps "`$service_name" --no-trunc || true

published_ports=`$(docker service inspect "`$service_name" --format '{{range .Endpoint.Ports}}{{.PublishedPort}}->{{.TargetPort}}/{{.Protocol}} {{end}}' 2>/dev/null || true)
if [ -n "`$published_ports" ]; then
    echo "Published ports for `"`$service_name`": `$published_ports"
else
    echo "No published ports detected for `"`$service_name`"."
fi
"@

Write-Host "Executing rolling deploy on $target ($RemotePath) with version $Version..."
Invoke-RemoteBash -Script $remoteScript | Out-Host

Write-Host "Remote deploy command completed."
