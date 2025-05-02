# IpConnectTracker – Local Development Setup

This project uses Docker Compose to run:

- PostgreSQL (with streaming replication: primary and replica)
- RabbitMQ
- Flyway for database migrations  
- `WriterService`: a .NET Core backend that listens for incoming events and persists them to PostgreSQL.

---

## How to Run

```bash
# Stop and remove containers and volumes (clean start)
docker compose down -v

# (Optional) Clean up any lingering anonymous volumes
docker volume prune -f

# Build and start containers in the background
docker compose up --build -d

# Run the backend service
dotnet run --project IpConnectTracker.WriterService
```

---

## Potential Issues

### 1. Docker volume not shared on macOS

**Error message:**
```
IpConnectTracker.WriterService.DataAccess.PostgreSQL/Migrations is not shared from the host and is not known to Docker
```

**Solution:**

1. Open Docker Desktop
2. Go to `Settings → Resources → File Sharing`
3. Click `Browse`, select your project folder (e.g., `../ipconnecttracker`)
4. Click the “+” button to add it
5. Click `Apply & Restart`

---

### 2. Permission issue with `copy_hba.sh`

If PostgreSQL fails to initialize, ensure that the `copy_hba.sh` script is executable:

```bash
chmod +x ./postgres-config/copy_hba.sh
```

Run this once per machine or after cloning the repo to avoid permission issues during container startup.

---

### 3. PostgreSQL fails with “data directory exists but is not empty”

This happens when the database volume exists but isn’t properly initialized (e.g., `pg_hba.conf` was placed in the data directory).

**Fix:**

```bash
docker compose down -v
docker volume prune -f
```

Then restart services:

```bash
docker compose up --build -d
```

---

## ✅ After Setup

Once everything is up and running, you should have:

- ✅ **Primary PostgreSQL** available at `localhost:5432`
- ✅ **Replica PostgreSQL** available at `localhost:5433`
- ✅ **RabbitMQ Management UI** at [http://localhost:15672](http://localhost:15672)
- ✅ **Flyway** automatically applies DB migrations from:
  - `IpConnectTracker.WriterService.DataAccess.PostgreSQL/Migrations`

---

## 🗝️ Default Credentials for Local Development

### 📦 PostgreSQL

| Component      | Host         | Port  | Database   | Username   | Password   |
|----------------|--------------|-------|------------|------------|------------|
| Primary DB     | `localhost`  | 5432  | `iptracker` | `app` | `secret` |
| Replica DB     | `localhost`  | 5433  | `iptracker` | `app` | `secret` |

> ℹ️ These values are defined in `docker-compose.yml` and `postgres-config/init.sql` (if present). You can change them as needed.

### 🐰 RabbitMQ

| UI URL                     | Username | Password |
|---------------------------|----------|----------|
| [http://localhost:15672](http://localhost:15672) | `guest`   | `guest`   |
