# Electricity Planner API

Backend API for recommending electricity billing plans from average monthly consumption (kWh) and tax group. Built for the LANACO practical assignment.

## Documentation (diagrams)

Diagrams for the assignment live under **`lib/Diagrams/`** (relative to this project folder):

| Diagram | Path |
| ------- | ---- |
| Entity-relationship (ERD) | `lib/Diagrams/ERD - Electricity Planner.png` |
| Class diagram | `lib/Diagrams/Class Diagram - Electricity Planner.png` |

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) matching the project (`net10.0` in the `.csproj`)
- [PostgreSQL](https://www.postgresql.org/download/) (local instance)
- Optional: [pgAdmin](https://www.pgadmin.org/) or another client to inspect the database

## Configuration

### Connection string (environment variables)

Do **not** commit database passwords in `appsettings*.json`. The API reads **`ConnectionStrings:DefaultConnection`** from configuration.

Set this **environment variable** (notice the **double underscore** `__` for nested keys):

| Variable | Purpose |
| -------- | ------- |
| `ConnectionStrings__DefaultConnection` | Full Npgsql connection string |

Example value (replace placeholders):

```text
Host=localhost;Port=5432;Database=YOUR_DATABASE;Username=YOUR_USER;Password=YOUR_PASSWORD
```

You can also use `Server=` instead of `Host=`; ensure the database exists or can be created, and the user has rights.

#### Windows (PowerShell, current session only)

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=electricity-planner;Username=postgres;Password=YOUR_PASSWORD"
dotnet run
```

#### IDE / `dotnet run` via `launchSettings.json` (optional)

You may add `ConnectionStrings__DefaultConnection` under `environmentVariables` in **`Properties/launchSettings.json`** for your profile — **only on your machine**, and avoid committing real passwords. Prefer PowerShell env vars or **User Secrets** for secrets shared across teammates without putting them in Git.

#### User Secrets (alternative to env vars)

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;..."
```

### Fallback

If neither env vars nor User Secrets supply the connection string, `GetConnectionString("DefaultConnection")` returns null and the app will fail at startup when configuring EF — this is intentional so misconfiguration is obvious.

## Running the API

From this project folder (`ElectricityPlanner.Api`):

```bash
dotnet restore
dotnet run
```

Ensure **`ConnectionStrings__DefaultConnection`** is set first (see above).

By default (see `Properties/launchSettings.json`) the app listens on:

- **HTTP:** `http://localhost:5226`
- **HTTPS:** `https://localhost:7184` (if you use the `https` launch profile)

Swagger UI (Development only): **`http://localhost:5226/swagger`**

## Docker (API + PostgreSQL)

From this project folder (where **`Dockerfile`** and **`docker-compose.yml`** live):

1. Copy **`env.example`** to **`.env`** and set `POSTGRES_USER`, `POSTGRES_PASSWORD`, and `POSTGRES_DB`. The file **`.env`** is listed in `.gitignore` and must not be committed.
2. Build and start:

```bash
docker compose build
docker compose up
```

- API: **`http://localhost:5226`** (mapped to container port 8080).
- Swagger: **`http://localhost:5226/swagger`** (`ASPNETCORE_ENVIRONMENT=Development` in compose).
- Inside the Compose network the database host is **`db`**; the API connection string is built from the same `.env` values as Postgres.

If port **5432** on your machine is already used by a local PostgreSQL instance, change the host mapping in `docker-compose.yml` for `db` (e.g. `"5433:5432"`). The API still uses **`Port=5432`** toward the `db` service.

## Database migrations and seed

On startup, **`Program.cs`**:

1. Applies pending EF Core migrations: `await db.Database.MigrateAsync();`
2. Runs **`SeedData.SeedAsync`** if the database has no plans yet (initial sample plans and tax groups).

### Applying migrations manually (optional)

```bash
dotnet tool install -g dotnet-ef   # once per machine
dotnet ef migrations add MigrationName   # when you change the model
dotnet ef database update             # uses the same connection string as the app
```

## CORS (React / Vite dev server)

The API allows the **`http://localhost:5173`** origin. Policy name: **`FrontendDev`** in `Program.cs`. Add more origins in `WithOrigins(...)` if needed.

## Pricing rules (summary)

1. **Energy subtotal** — progressive tiers (thresholds as cumulative ceilings; last tier may use `null`).
2. **Plan discount** — `energyAfterDiscount = energySubtotal × (1 − planDiscount)`.
3. **Eco** — `EcoTax × kWh`.
4. **VAT** — `(energyAfterDiscount + ecoTaxTotal) × VAT`.
5. **Grand total** — sum of discounted energy, eco, and VAT.

Recommendation picks the plan with the lowest **`grandTotal`** for the same input.

## Example HTTP requests

Base URL: **`http://localhost:5226`**. Use **`Content-Type: application/json`** for POST/PUT bodies.

### List plans

```http
GET /plans HTTP/1.1
Host: localhost:5226
```

### Get one plan

```http
GET /plans/1 HTTP/1.1
Host: localhost:5226
```

### Recommendation

```http
POST /recommendation HTTP/1.1
Host: localhost:5226
Content-Type: application/json

{
  "kwh": 350,
  "taxGroup": "household"
}
```

### List tax groups

```http
GET /tax-groups HTTP/1.1
Host: localhost:5226
```

## Tech stack

| Layer | Technology |
| ----- | ---------- |
| API | ASP.NET Core |
| ORM | Entity Framework Core |
| Database | PostgreSQL (Npgsql) |
| API docs | Swagger / Swashbuckle |
| Tests | xUnit |

## Project structure (high level)

- `Controllers/` — HTTP endpoints (`plans`, `tax-groups`, `recommendation`)
- `Application/` — services (`PricingService`), DTOs
- `Domain/` — entities (`Plan`, `PricingTier`, `TaxGroup`)
- `Infrastructure/Data/` — `AppDbContext`, seed

## Running automated tests

If you have a companion xUnit project referencing this API:

```bash
dotnet test path/to/ElectricityPlanner.Tests.csproj
```
