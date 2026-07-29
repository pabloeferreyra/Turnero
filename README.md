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

<h2>🗄️ Redis Cache (distribuido)</h2>

Turnero utiliza **Redis** como caché distribuido de dos niveles para acelerar la carga de datos y reducir la carga en PostgreSQL.

### Arquitectura (two-tier cache)

```
┌─────────────┐     ┌──────────────┐     ┌────────────┐
│   Cliente   │────▶│ SignalR Hub  │────▶│ PostgreSQL │
└─────────────┘     └──────────────┘     └─────┬──────┘
       ▲                                       │
       │                              ┌────────▼───────┐
       │                              │  L1: IMemoryCache │
       │                              │  (local, ultra-fast)
       │                              └────────┬───────┘
       │                              ┌────────▼───────┐
       │                              │  L2: Redis Cache│
       │      ┌───────────────────┐   │  (distribuido) │
       └──────│ Pub/Sub Invalida  │   └────────┬───────┘
              │ cross-instancia   │            │
              └───────────────────┘   ┌────────▼───────┐
                                      │  PostgreSQL    │
                                      │  (fallback/DB) │
                                      └────────────────┘
```

*   **L1 — IMemoryCache**: Caché local en memoria de cada instancia. Ultra-rápida, sin serialización.
*   **L2 — Redis**: Caché distribuido compartido entre todas las instancias. Usa serialización JSON.
*   **Fallback**: Si Redis no está disponible, la app funciona solo con PostgreSQL (degradación graceful vía `AbortOnConnectFail=false`).

### Variables de entorno

| Variable | Default | Descripción |
|---|---|---|
| `Redis__ConnectionString` | `localhost:6379` | Conexión a Redis. En Docker Swarm usar `redis:6379`. |

> En .NET, el `__` (doble underscore) se traduce a `:` en la configuración. `Redis__ConnectionString` → `IConfiguration["Redis:ConnectionString"]`.

### Cache keys y TTLs

| Key | Contenido | TTL (Redis) | TTL (Memoria) |
|---|---|---|---|
| `medics` | Lista de médicos | 1 hora | Sin expiración (invalidación explícita) |
| `timeTurns` | Lista de horarios | 1 hora | Sin expiración (invalidación explícita) |
| `turns:{date}:{medicId}` | Turnos del día | 2 minutos | 30 segundos |

### Invalidación de caché

Cuando se crea, edita o elimina un turno/médico/horario:

1. Se borra la key de **Redis** (`KeyDelete`)
2. Se borra de **IMemoryCache** local (`Remove`)
3. Se publica un mensaje en el canal **Pub/Sub** `cache:invalidate`
4. Todas las demás instancias reciben el mensaje y borran su **IMemoryCache** local

Esto garantiza que el caché esté siempre consistente entre múltiples réplicas.

### Desarrollo local

**Opción 1 — Script con Docker automático** (recomendado):

Los scripts `start-dev.ps1` y `start-dev.sh` detectan si Redis está corriendo. Si no, inician automáticamente un contenedor Docker:

```powershell
# Windows
./scripts/start-dev.ps1
```

```bash
# Linux/macOS
chmod +x ./scripts/start-dev.sh
./scripts/start-dev.sh
```

Si Docker no está disponible, la app arranca igual (solo con IMemoryCache).

**Opción 2 — Redis nativo**:

```bash
# Linux (apt)
sudo apt install redis-server
sudo systemctl start redis-server

# macOS (Homebrew)
brew install redis
brew services start redis
```

**Opción 3 — Docker manual**:

```bash
docker run -d --name turnero-redis-dev -p 6379:6379 redis:7-alpine
```

### Redis Commander (UI web)

Interfaz gráfica para inspeccionar keys, valores y monitorear el caché en tiempo real.

```bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d
```

Luego abrir http://localhost:8081

### Docker Compose (desarrollo)

Redis ya está incluido en `docker-compose.yml` con healthcheck:

```yaml
redis:
  image: redis:7-alpine
  ports:
    - "6379:6379"
  healthcheck:
    test: ["CMD", "redis-cli", "ping"]
```

Usar `Redis__ConnectionString=redis:6379` en `.env` cuando se corre con Docker Compose.

### Docker Swarm (producción)

Redis está incluido en `docker-stack.prod.yml` y `docker-stack.tls.yml` con:

*   Volumen persistente `redis-data:/data`
*   Sin puerto externo (solo red interna overlay — seguridad)
*   1 réplica en el manager node
*   0.25 CPU / 256 MB RAM límite

En `.env` de producción (Swarm):

```
Redis__ConnectionString=redis:6379
```

El script de deploy (`deploy.sh`) valida y exporta automáticamente `Redis__ConnectionString`. Si no está definida, usa `redis:6379` como default.

### Comandos útiles (redis-cli)

```bash
# Ver todas las keys (cuidado en producción con KEYS *)
redis-cli KEYS "*"

# Ver keys de turnos
redis-cli KEYS "turns:*"

# Ver el valor de una key
redis-cli GET "medics"

# Ver TTL de una key (segundos restantes)
redis-cli TTL "medics"

# Borrar una key específica
redis-cli DEL "medics"

# Borrar todas las keys del caché (solo desarrollo)
redis-cli FLUSHDB

# Monitorear operaciones en vivo
redis-cli MONITOR

# Ver estadísticas de memoria
redis-cli INFO memory

# Ver el canal Pub/Sub (suscribirse a invalidaciones)
redis-cli SUBSCRIBE "cache:invalidate"
```

### Paquetes NuGet

*   `StackExchange.Redis` (v3.0.17+) — Cliente Redis para .NET
*   `Microsoft.Extensions.Hosting.Abstractions` — Para `IHostedService` (invalidación cross-instancia)

### Servicios relacionados

| Archivo | Propósito |
|---|---|
| `Turnero.SL/Services/RedisConnectionService.cs` | Singleton que maneja `ConnectionMultiplexer` |
| `Turnero.SL/Services/RedisCacheService.cs` | Operaciones Get/Set/Remove/Publish/Subscribe (sync + async) |
| `Turnero.SL/Services/CacheInvalidationHostedService.cs` | Escucha Pub/Sub para invalidación cross-instancia |
| `Turnero.SL/Services/Repositories/RepositoryBase.cs` | Two-tier cache (IMemoryCache + Redis) en repositorios |
| `Turnero.SL/Services/TurnsServices/GetTurnDTOServices.cs` | Cache de consultas de turnos con TTL corto |
| `Controllers/TurneroBaseController.cs` | Helper `InvalidateCacheAsync` para controladores |

---

<h2>🛡️ License:</h2>

This project is licensed under the MIT License

<h2>💖Like my work?</h2>

If you have any questions or comments about the application you can contact the author through the following email address: pabloeferreyra@gmail.com.<p>![Open Collective sponsors](https://img.shields.io/opencollective/sponsors/patreon?style=flat-square&amp;logo=patreon&amp;label=Patreon&amp;link=https%3A%2F%2Fpatreon.com%2Fpfsoftware)<br>[![Invitame un café en cafecito.app](https://cdn.cafecito.app/imgs/buttons/button_5.svg)](https://cafecito.app/pfsoftware)</p>
