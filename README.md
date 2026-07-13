<h1 align="center" id="title">Turnero</h1>

<p align="center"><img src="https://socialify.git.ci/pabloeferreyra/Turnero/image?description=1&amp;descriptionEditable=A%20web%20application%20that%20allows%20users%20to%20manage%20shifts%20for%20different%20services.&amp;language=1&amp;name=1&amp;owner=1&amp;pattern=Signal&amp;theme=Dark" alt="project-image"></p>

<p align="center"><img src="https://img.shields.io/github/actions/workflow/status/pabloeferreyra/Turnero/DeployProd.yml?style=flat-square&amp;logo=dotnet" alt="shields"></p>

<h2>🚀 Demo</h2>

[turnero.dev.ar](turnero.dev.ar)

<H2><a href="https://pabloferreyra.mintlify.site">Wiki</a></H2>
  
<h2>🧐 Features</h2>

Here're some of the project's best features:

*   User Roles
*   Multi Platform
*   PWA
*   Desktop Notifications

  
  
<h2>💻 Built with</h2>

<a href="https://docs.microsoft.com/en-us/dotnet/csharp/" target="_blank" rel="noreferrer"><img src="https://raw.githubusercontent.com/danielcranney/readme-generator/main/public/icons/skills/csharp-colored.svg" width="36" height="36" alt="C#" /></a><a href="https://developer.mozilla.org/en-US/docs/Web/JavaScript" target="_blank" rel="noreferrer"><img src="https://raw.githubusercontent.com/danielcranney/readme-generator/main/public/icons/skills/javascript-colored.svg" width="36" height="36" alt="JavaScript" /></a><a href="https://developer.mozilla.org/en-US/docs/Glossary/HTML5" target="_blank" rel="noreferrer"><img src="https://raw.githubusercontent.com/danielcranney/readme-generator/main/public/icons/skills/html5-colored.svg" width="36" height="36" alt="HTML5" /></a><a href="https://jquery.com/" target="_blank" rel="noreferrer"><img src="https://raw.githubusercontent.com/danielcranney/readme-generator/main/public/icons/skills/jquery-colored.svg" width="36" height="36" alt="JQuery" /></a><a href="https://getbootstrap.com/" target="_blank" rel="noreferrer"><img src="https://raw.githubusercontent.com/danielcranney/readme-generator/main/public/icons/skills/bootstrap-colored.svg" width="36" height="36" alt="Bootstrap" /></a><a href="https://www.postgresql.org/" target="_blank" rel="noreferrer"><img src="https://raw.githubusercontent.com/danielcranney/readme-generator/main/public/icons/skills/postgresql-colored.svg" width="36" height="36" alt="PostgreSQL" /></a><a href="https://dotnet.microsoft.com/en-us/" target="_blank" rel="noreferrer"><img src="https://raw.githubusercontent.com/danielcranney/readme-generator/main/public/icons/skills/dot-net-colored.svg" width="36" height="36" alt=".NET" /></a>

*   Net 8
*   Razor
*   PostgreSQL
*   SignalR
*   Javascript
*   Bootstrap

<h2>⚙️ Environment variables (Windows + Linux)</h2>

You can run the project without User Secrets by defining environment variables.

Quick start with .env file:

1. Copy `.env.example` to `.env` and complete values.
2. Start using one of the helper scripts:

PowerShell (Windows):

```powershell
./scripts/start-dev.ps1
```

Bash (Linux/macOS):

```bash
chmod +x ./scripts/start-dev.sh
./scripts/start-dev.sh
```

Required:

*   ConnectionStrings__LocalConnection

Firebase credentials (path-only pattern):

*   Firebase__CredentialsPath
*   GOOGLE_APPLICATION_CREDENTIALS

Authentication variables used by the app:

*   Authentication__ValidIssuer
*   Authentication__Audience
*   Authentication__TokenUri
*   Authentication__TokenCode
*   Authentication__TokenReset

PowerShell (Windows):

```powershell
$env:ConnectionStrings__LocalConnection = "Host=localhost;Port=5432;Database=turnero;Username=turnero;Password=turnero_pwd"
$env:Firebase__CredentialsPath = "C:\secrets\firebase.json"
$env:Authentication__ValidIssuer = "https://securetoken.google.com/your-project"
$env:Authentication__Audience = "your-project"
dotnet run
```

Bash (Linux/macOS):

```bash
export ConnectionStrings__LocalConnection="Host=localhost;Port=5432;Database=turnero;Username=turnero;Password=turnero_pwd"
export Firebase__CredentialsPath="/run/secrets/firebase.json"
export Authentication__ValidIssuer="https://securetoken.google.com/your-project"
export Authentication__Audience="your-project"
dotnet run
```

Note:

*   In Development, User Secrets still works as an optional fallback.
*   Inline JSON credentials in environment variables are disabled to reduce secret exposure risk.

<h2>🐳 Docker (using external database)</h2>

This setup runs only the web app in Docker and keeps PostgreSQL outside Docker.

Recommended TLS approach:

*   Prefer a reverse proxy such as Nginx or Traefik in front of the app.
*   Avoid baking Let's Encrypt certificates into the image.
*   If you want Kestrel to terminate TLS directly, use [docker-stack.tls.yml](docker-stack.tls.yml) and mount the host certificate directory read-only.

1. Ensure `.env` has `ConnectionStrings__PostgresConnection` with your current external DB connection string.
2. Provide a firebase service account file path through `FIREBASE_CREDENTIALS_FILE`.

Windows PowerShell:

```powershell
$env:FIREBASE_CREDENTIALS_FILE = "D:/UserSecrets/aspnet-Turnero-1D8EA02B-D124-439A-B5F8-DE2044EFFABA/firebase.json"
docker compose up --build
```

Linux/macOS Bash:

```bash
export FIREBASE_CREDENTIALS_FILE="/opt/secrets/firebase.json"
docker compose up --build
```

Application URL:

*   http://localhost:8080

Production compose (healthcheck + resource limits):

```powershell
$env:FIREBASE_CREDENTIALS_FILE = "D:/UserSecrets/aspnet-Turnero-1D8EA02B-D124-439A-B5F8-DE2044EFFABA/firebase.json"
docker compose -f docker-compose.prod.yml up --build -d
```

```bash
export FIREBASE_CREDENTIALS_FILE="/opt/secrets/firebase.json"
docker compose -f docker-compose.prod.yml up --build -d
```

Direct TLS stack with mounted Let's Encrypt certs:

```bash
export LETSENCRYPT_DOMAIN="vps-1821822-x.dattaweb.com"
export FIREBASE_CREDENTIALS_FILE="/opt/secrets/firebase.json"
docker stack deploy -c docker-stack.tls.yml turnero
```

You can also set `LETSENCRYPT_DOMAIN` in `.env` and let the deploy script load it automatically.

Notes for direct TLS:

*   The certificate directory must exist on every Swarm node that can run the task.
*   The app listens on `443` and `8080` in that stack.
*   Healthcheck uses `curl -k` because the certificate is validated against the real domain, not `localhost`.

<h2>🔁 Zero-downtime updates (2 instances)</h2>

For true rolling updates (keep one instance running while the next starts), use Docker Swarm with `docker-stack.prod.yml`.

Initial setup (Linux server):

```bash
docker swarm init
export FIREBASE_CREDENTIALS_FILE="/opt/secrets/firebase.json"
docker build -t turnero-app:prod .
docker stack deploy -c docker-stack.prod.yml turnero
```

Update to a new version without downtime:

```bash
docker build -t turnero-app:prod-v3.0.2 .
export TURNERO_IMAGE="turnero-app:prod-v3.0.2"
export FIREBASE_CREDENTIALS_FILE="/opt/secrets/firebase.json"
docker stack deploy -c docker-stack.prod.yml turnero
```

Verify rollout:

```bash
docker service ls
docker service ps turnero_turnero-app
```

One-command deploy script:

```bash
chmod +x ./scripts/deploy.sh
./scripts/deploy.sh v3.0.2 /opt/secrets/firebase.json
```

Optional env vars:

*   `STACK_NAME` (default: `turnero`)
*   `STACK_FILE` (default: `docker-stack.prod.yml`)
*   `IMAGE_REPO` (default: `turnero-app`)
*   `FIREBASE_CREDENTIALS_FILE` (if you prefer not to pass arg2)

Deploy from Windows to Linux over SSH:

```powershell
./scripts/deploy-remote.ps1 -RemoteHost "your-server" -User "deploy" -Version "v3.0.2" -RemotePath "/opt/turnero" -FirebaseCredentialsFile "/opt/secrets/firebase.json" -SshPort 2222
```

This command now builds the Docker image locally, transfers it to the remote host (`docker save | docker load`), and runs `docker stack deploy` remotely.
After deploy, the script prints the published ports for the target service.

Prerequisites:

*   Local Docker daemon must be running.
*   Remote host must be a Docker Swarm manager node.

Sync `.env` only when changed (hash comparison):

```powershell
./scripts/deploy-remote.ps1 -RemoteHost "your-server" -User "deploy" -Version "v3.0.2" -RemotePath "/opt/turnero" -FirebaseCredentialsFile "/opt/secrets/firebase.json" -SyncEnv
```

Sync `.env` with automatic remote backup before replace:

```powershell
./scripts/deploy-remote.ps1 -RemoteHost "your-server" -User "deploy" -Version "v3.0.2" -RemotePath "/opt/turnero" -FirebaseCredentialsFile "/opt/secrets/firebase.json" -SyncEnv -BackupEnv
```

Backup retention example (keep last 20 backups):

```powershell
./scripts/deploy-remote.ps1 -RemoteHost "your-server" -User "deploy" -Version "v3.0.2" -RemotePath "/opt/turnero" -FirebaseCredentialsFile "/opt/secrets/firebase.json" -SyncEnv -BackupEnv -BackupEnvRetention 20
```

Optional params:

*   `-StackName` (default: `turnero`)
*   `-StackFilePath` (default: `docker-stack.prod.yml`)
*   `-UseTls` (uses `docker-stack.tls.yml` unless `-StackFilePath` is explicitly provided)
*   `-ImageRepo` (default: `turnero-app`)
*   `-SshKeyPath` (for key-based auth)
*   `-SshPort` (default: `22`)
*   `-SyncEnv` (copies `.env` only if content changed)
*   `-EnvFilePath` (default: `.env`)
*   `-BackupEnv` (creates remote backup: `.env.bak.YYYYMMDDHHMMSS` before overwrite)
*   `-BackupEnvRetention` (default: `10`, use `0` to disable pruning)

TLS deploy example (binds Let's Encrypt certs from host):

```powershell
./scripts/deploy-remote.ps1 -RemoteHost "your-server" -User "deploy" -Version "v3.0.2" -RemotePath "/opt/turnero" -FirebaseCredentialsFile "/opt/secrets/firebase.json" -SyncEnv -UseTls
```

For `-UseTls`, `.env` must include `LETSENCRYPT_DOMAIN`, and on the remote host this path must exist:

*   `/etc/letsencrypt/live/<LETSENCRYPT_DOMAIN>/fullchain.pem`
*   `/etc/letsencrypt/live/<LETSENCRYPT_DOMAIN>/privkey.pem`

Note: `docker-stack.tls.yml` mounts `/etc/letsencrypt` read-only (not only `/etc/letsencrypt/live/<domain>`), because Let's Encrypt files in `live/` are commonly symlinks to `archive/`.

Environment variable fallback for SSH port:

*   `DEPLOY_SSH_PORT` (preferred)
*   `SSH_PORT` (fallback)

The stack is configured with:

*   `replicas: 2`
*   `update_config.parallelism: 1`
*   `update_config.order: start-first`
*   `failure_action: rollback`
*   Resource limits are intentionally low so the stack can run on a single-CPU host.
*   Set `APP_PUBLISHED_PORT` to avoid conflicts when another stack already uses the same host port.
*   For the TLS stack, set `APP_HTTPS_PORT` if you need a non-standard published HTTPS port.

<h2>🛡️ License:</h2>

This project is licensed under the MIT License

<h2>💖Like my work?</h2>

If you have any questions or comments about the application you can contact the author through the following email address: pabloeferreyra@gmail.com.<p>![Open Collective sponsors](https://img.shields.io/opencollective/sponsors/patreon?style=flat-square&amp;logo=patreon&amp;label=Patreon&amp;link=https%3A%2F%2Fpatreon.com%2Fpfsoftware)<br>[![Invitame un café en cafecito.app](https://cdn.cafecito.app/imgs/buttons/button_5.svg)](https://cafecito.app/pfsoftware)</p>
