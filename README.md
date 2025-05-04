# IpConnectTracker – Local Development Setup

This project uses Docker Compose to run:

- PostgreSQL (with streaming replication: primary and replica)
- RabbitMQ
- Flyway for database migrations  
- `WriterService`: a .NET Core backend that listens for incoming events and writes them to PostgreSQL.
- `ReaderService.Api`: a .NET Web API for querying connection history

---

## How to Run

```bash
# Stop and remove containers and volumes (clean start)
docker compose down -v

# Build and start containers
docker compose up --build

# Or build and start containers in the background
docker compose up --build -d
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

> ℹ️ These values are defined in `docker-compose.yml`

### 🐰 RabbitMQ

| UI URL                     | Username | Password |
|---------------------------|----------|----------|
| [http://localhost:15672](http://localhost:15672) | `guest`   | `guest`   |

---

## 🔍 ReaderService API

Project: `IpConnectTracker.ReaderService.Api`

Base URL (by default): `http://localhost:5109`
Swagger URL: `http://localhost:5109/swagger/index.html`

### 📘 Endpoints

| HTTP Method | URL | Description |
|-------------|-----|-------------|
| `GET` | `/api/connection-events/users/by-ip-prefix?prefix=192&skip=0&take=100` | Find users by IP prefix |
| `GET` | `/api/connection-events/users/{userId}/ips` | Get all IPs used by the user |
| `GET` | `/api/connection-events/users/{userId}/latest` | Get the last connection of a user (IP + timestamp) |
| `GET` | `/api/connection-events/users/{userId}/latest-by-ip?ip=192.168.1.1` | Get the last connection timestamp for a user for a specific IP |

---

## Populating the Database with test data

You can use the `IpConnectTracker.RabbitMqPublisher.Cli` tool to send simulated connection events and populate the database with random user/IP pairs.

### How to Run

Make sure Docker services (PostgreSQL, RabbitMQ, WriterService etc) are up and running.
Then run the publisher CLI:

```bash
dotnet run --project src/IpConnectTracker.RabbitMqPublisher.Cli --count 1000 --user-count 100 --queue ip_connects
```

### ⚙️ Command-line Arguments

| Argument        | Description                                  | Default        |
|-----------------|----------------------------------------------|----------------|
| `--count`       | Number of connection events to send          | 100            |
| `--user-count`  | Number of unique user ids in the events      | 10             |
| `--queue`       | RabbitMQ queue name                          | ip_connects    |


## Roadmap

Planned improvements and next steps:

- **Add metrics** — to compare performance of different approaches and monitor system health;
- **Optimize IP prefix search in PostgreSQL** — consider adding an index on the `ip_address` field to speed up prefix queries if it doesn’t degrade write performance;
- **Evaluate Elasticsearch** — as an alternative read source for advanced or large-scale IP prefix search;
- **Introduce retry logic and dead-letter queue** — to safely handle transient failures and not processed events in the writer pipeline;
- **Add integration tests for failure scenarios** — simulate DB/RabbitMQ/service outages to verify resilience and recovery behavior;
- **Add persistent log storage** — centralize logs with tools like ELK stack for observability and debugging.
