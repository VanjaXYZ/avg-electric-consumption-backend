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

### JWT (startup + login + admin writes)

The host loads **`Jwt`** settings at startup, validates bearer tokens on protected actions, and issues tokens from **`POST /auth/login`**.

**Authorization:** **`POST`**, **`PUT`**, and **`DELETE`** on **`/plans`** and **`/tax-groups`**, and **`GET /analytics/plan-selections/summary`** and **`GET /analytics/plan-selections/trends`**, require an authenticated user with role **`Admin`**. **`GET`** on **`/plans`** and **`/tax-groups`**, **`POST /recommendation`**, **`POST /recommendation/email`**, and **`POST /auth/login`** are anonymous.

Set these via environment variables (double underscore) or User Secrets under section **`Jwt`**. Names must match **`JwtOptions`** in code: **`Jwt:SecretKey`** and **`Jwt:ExpiryMinutes`**, or the app will not pick up the key or lifetime you expect.

| Key | Maps to env var | Notes |
| --- | ---------------- | ----- |
| `Jwt:Issuer` | `Jwt__Issuer` | Token issuer |
| `Jwt:Audience` | `Jwt__Audience` | Token audience |
| `Jwt:SecretKey` | `Jwt__SecretKey` | **At least 32 characters** |
| `Jwt:ExpiryMinutes` | `Jwt__ExpiryMinutes` | Access token lifetime (minutes) |

```powershell
$env:Jwt__Issuer = "ElectricityPlanner"
$env:Jwt__Audience = "ElectricityPlanner"
$env:Jwt__SecretKey = "your-development-secret-key-32chars-min!!"
$env:Jwt__ExpiryMinutes = "60"
```

```bash
dotnet user-secrets set "Jwt:Issuer" "ElectricityPlanner"
dotnet user-secrets set "Jwt:Audience" "ElectricityPlanner"
dotnet user-secrets set "Jwt:SecretKey" "your-development-secret-key-32chars-min!!"
dotnet user-secrets set "Jwt:ExpiryMinutes" "60"
```

**Login:** `POST /auth/login` with JSON `{ "username", "password" }`. After the first migration + seed, demo users are **`admin`** / **`lanaco2026`** (role `Admin`) and **`user`** / **`123456`** (role `User`). Send the returned JWT as the header **`Authorization: Bearer <token>`** on admin mutations.

**Swagger / Postman:** In Swagger **Authorize**, paste **only the JWT**. If **`POST /plans`** (or similar) still returns **401** and the browser’s Network tab shows **no `Authorization` header**, use **Postman** or another HTTP client—the API is correct when that header is present.

### SMTP (optional — send recommendation by email)

**`POST /recommendation/email`** sends an HTML summary to **`ToEmail`** after computing the same recommendation as **`POST /recommendation`**. If **`Smtp:Host`** is not set, that endpoint returns **`400`** with a message to configure SMTP (the rest of the API still starts).

| Key | Maps to env var |
| --- | ---------------- |
| `Smtp:Host` | `Smtp__Host` |
| `Smtp:Port` | `Smtp__Port` (default **587** in code if omitted) |
| `Smtp:Username` | `Smtp__Username` |
| `Smtp:Password` | `Smtp__Password` |
| `Smtp:FromEmail` | `Smtp__FromEmail` |
| `Smtp:FromName` | `Smtp__FromName` |

Use [Mailtrap](https://mailtrap.io) (or similar) for development: copy SMTP host, port, username, and password from the inbox settings into User Secrets. Port **465** uses implicit SSL; **587** uses STARTTLS.

### Testing email (Mailtrap Sandbox)

These steps exercise **`POST /recommendation/email`** without sending real internet mail from the sandbox.

1. Sign in to [Mailtrap](https://mailtrap.io) and open **Email Testing** → your **Sandbox** inbox (e.g. *My Sandbox*).
2. Go to **Integration** → sub-tab **SMTP** (not the Transactional **Email API** / Bearer flow — this API connects with **SMTP** credentials via MailKit).
3. Copy **Host**, **Port**, **Username**, and **Password** from that screen. Use port **587** (STARTTLS) unless your network blocks it, then try **2525**. Reveal or copy the **full** password (the masked value is not enough).
4. From this project folder, set User Secrets (same keys as the SMTP table above; `Smtp:Username` matches the property name in code — not `Smtp:User`):

   ```powershell
   dotnet user-secrets set "Smtp:Host" "sandbox.smtp.mailtrap.io"
   dotnet user-secrets set "Smtp:Port" "587"
   dotnet user-secrets set "Smtp:Username" "<paste from Mailtrap>"
   dotnet user-secrets set "Smtp:Password" "<paste from Mailtrap>"
   dotnet user-secrets set "Smtp:FromEmail" "noreply@example.com"
   dotnet user-secrets set "Smtp:FromName" "Electricity Planner"
   ```

   Replace `FromEmail` / `FromName` as you like for development; the sandbox mainly needs a well-formed **From** address.

5. Configure **PostgreSQL** and **JWT**, then run **`dotnet run`** (see [Running the API](#running-the-api)).
6. Send a request with **`Content-Type: application/json`**, e.g. Swagger **`POST /recommendation/email`** or:

   ```http
   POST /recommendation/email HTTP/1.1
   Host: localhost:5226
   Content-Type: application/json

   {
     "kwh": 3500,
     "taxGroup": "household",
     "toEmail": "you@example.com"
   }
   ```

   Seed tax groups include **`household`** and **`business`**.

7. Refresh the **Mailtrap Sandbox** inbox in the browser — the message should appear there shortly.

**Sandbox vs real inbox:** Email Testing **does not deliver** to the address in `toEmail` on the public internet; messages stay in the Mailtrap UI for inspection. If the message shows in Mailtrap but not in Gmail, that is expected. For real delivery, use **Mailtrap Sending** / another transactional SMTP provider, **verify your domain**, and point **`Smtp:*`** at that provider’s SMTP host and credentials.

## Running the API

From this project folder (`ElectricityPlanner.Api`):

```bash
dotnet restore
dotnet run
```

Ensure **`ConnectionStrings__DefaultConnection`** and **JWT** settings are set first (see above). **SMTP** is only required if you use **`POST /recommendation/email`**.

By default (see `Properties/launchSettings.json`) the app listens on:

- **HTTP:** `http://localhost:5226`
- **HTTPS:** `https://localhost:7184` (if you use the `https` launch profile)

Swagger UI (Development only): **`http://localhost:5226/swagger`**

## Docker (API + PostgreSQL)

From this project folder (where **`Dockerfile`** and **`docker-compose.yml`** live):

1. Copy **`env.example`** to **`.env`** and set `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, and **`JWT_SECRET_KEY`** (at least 32 characters). Optional: `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_EXPIRY_MINUTES`, and **`SMTP_*`** keys if you use recommendation email inside Docker. The file **`.env`** is listed in `.gitignore` and must not be committed.
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
2. Runs **`SeedData.SeedAsync`**: seeds **`AppUsers`** when empty, then sample **tax groups** and **plans** when there are no plans yet.

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

### Recommendation by email

Same calculation as **`POST /recommendation`**, plus an HTML email to **`toEmail`**. Returns **`502`** if SMTP fails after validation. Logs analytics the same way as **`POST /recommendation`** after a successful send.

```http
POST /recommendation/email HTTP/1.1
Host: localhost:5226
Content-Type: application/json

{
  "kwh": 350,
  "taxGroup": "household",
  "toEmail": "you@example.com"
}
```

### List tax groups

```http
GET /tax-groups HTTP/1.1
Host: localhost:5226
```

### Delete plan (soft delete)

**`DELETE /plans/{id}`** (Admin) sets **`IsDeleted`** on the plan row instead of removing it. The plan disappears from **`GET /plans`**, **`GET /plans/{id}`**, and **`POST /recommendation`**, but **analytics** rows that reference it stay valid (no foreign key errors). Repeating **`DELETE`** on an already deleted plan returns **`204`** (idempotent).

### Delete tax group (soft delete)

**`DELETE /tax-groups/{id}`** (Admin) sets **`IsDeleted`** on the tax group row. It no longer appears in **`GET /tax-groups`** or in **`POST /recommendation`** (unknown tax group), while **analytics** rows that reference it keep a valid FK. Second **`DELETE`** on the same id returns **`204`**.

### Analytics — plan selection tracking

Each successful **`POST /recommendation`** or **`POST /recommendation/email`** (after a successful email send) persists one row in **`PlanSelectionEvents`**: UTC time, **kWh**, **tax group id**, **recommended plan id**, and **recommended grand total** at the time of the response (for behaviour analytics). No extra request body is required beyond the normal recommendation payload.

**Summary (Admin only):** aggregate counts per recommended plan:

```http
GET /analytics/plan-selections/summary HTTP/1.1
Host: localhost:5226
Authorization: Bearer <admin_jwt>
```

Use the **`token`** from **`POST /auth/login`** as user **`admin`**. Response: JSON array with **`planId`**, **`planName`**, **`selectionCount`** (descending by count). Same **`Admin`** JWT as for **`POST /plans`**; if Swagger omits **`Authorization`**, use Postman.

**Trends by date range (Admin only):** counts per **UTC calendar day** and per **recommended plan** between **`from`** and **`to`** (inclusive). Query parameters use ISO dates `yyyy-MM-dd`. Maximum span: **366** days. Returns **`400`** if `from` is after `to`, or if the range is too wide.

```http
GET /analytics/plan-selections/trends?from=2026-05-01&to=2026-05-31 HTTP/1.1
Host: localhost:5226
Authorization: Bearer <admin_jwt>
```

Use **`GET`** with **no body**; send the JWT only in **`Authorization`**. Response rows: **`date`** (midnight UTC for that day bucket), **`planId`**, **`planName`**, **`count`**, ordered by date then plan name — suitable for line or bar charts in a frontend.

## Tech stack

| Layer | Technology |
| ----- | ---------- |
| API | ASP.NET Core |
| ORM | Entity Framework Core |
| Database | PostgreSQL (Npgsql) |
| API docs | Swagger / Swashbuckle |
| Tests | xUnit |

## Project structure (high level)

- `Controllers/` — HTTP endpoints (`plans`, `tax-groups`, `recommendation`, `auth`, `analytics`)
- `Application/` — services (`PricingService`, `RecommendationService`, `PlanSelectionAnalytics`), DTOs
- `Domain/` — entities (`Plan`, `PricingTier`, `TaxGroup`, `PlanSelectionEvent`, `AppUsers`)
- `Infrastructure/Data/` — `AppDbContext`, seed
- `Infrastructure/Email/` — SMTP recommendation email sender
- `ElectricityPlanner.Tests/` — xUnit: unit tests (`PricingService`, …) and **EF InMemory integration-style tests** (`RecommendationService`, `PlanSelectionAnalytics` with `SeedData`; no Docker)

## Running automated tests

From the API project folder, the test project lives at **`ElectricityPlanner.Tests/ElectricityPlanner.Tests.csproj`**:

```bash
dotnet test ElectricityPlanner.Tests/ElectricityPlanner.Tests.csproj
```

**Integration-style tests** use **EF Core InMemory** plus the same services and `SeedData` as the API (no `WebApplicationFactory`, no PostgreSQL, no Docker). They validate recommendation and analytics persistence against a real `AppDbContext` instance.
