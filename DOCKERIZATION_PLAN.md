# Dockerization Plan — ASP.NET Core API + React Client + SQL Server

A step-by-step plan for containerizing the full stack: React (Nginx) → ASP.NET Core API → SQL Server, with automatic EF Core migrations and no manual setup required to run locally.

## Overview

```
┌─────────────┐      /api/*      ┌─────────────┐      ┌─────────────┐
│   client     │ ───────────────▶ │     api      │ ───▶ │     db       │
│  (Nginx +    │  :3000 exposed   │ (ASP.NET     │      │ (SQL Server  │
│   React)     │                  │  Core)       │      │  2022)       │
└─────────────┘                  └─────────────┘      └─────────────┘
                                    :8080 exposed
```

- `client` — React app built to static files, served by Nginx, reverse-proxies `/api/*` to the API container (no CORS needed for the app itself)
- `api` — ASP.NET Core Web API, runs EF Core migrations automatically on startup
- `db` — SQL Server 2022, data persisted in a named volume

---

## Step 1 — Add `.dockerignore` (API)

Exclude local build artifacts from the Docker build context.

```
bin/
obj/
**/bin
**/obj
.env
.git
.vs
.vscode
*.user
**/TestResults
```

## Step 2 — Write the API Dockerfile (multi-stage)

Place at the solution root.

- Stage 1 (`sdk:8.0`): restore + publish
- Stage 2 (`aspnet:8.0`): copy published output, expose port `8080`
- Copy `.csproj` files before the rest of the source for Docker layer caching (faster rebuilds when only `.cs` files change)

## Step 3 — Confirm Kestrel binds correctly

Ensure the API listens on `8080` inside the container via `ASPNETCORE_URLS=http://+:8080` (set in Compose, not hardcoded).

## Step 4 — Add automatic migrations on startup

In `Program.cs`, after `var app = builder.Build();`:
- Resolve `AppDbContext` from a scope
- Call `db.Database.Migrate()`
- Wrap in a retry loop (5 attempts, 5s delay) to handle SQL Server not being ready yet on first boot

This removes any manual migration step — anyone who clones the repo just runs the app and the schema is created/updated automatically.

## Step 5 — Write `docker-compose.yml` (API + SQL Server)

Three services:
- `api` — builds from root Dockerfile, exposes `8080`, depends on `db` being healthy
- `db` — SQL Server image, `MSSQL_SA_PASSWORD` from `.env`, named volume `sql_data`, healthcheck via `sqlcmd`
- `sql_data` volume for persistence across restarts

Healthcheck + `depends_on: condition: service_healthy` is required — without it, the API container starts before SQL Server accepts connections and crashes on first request.

## Step 6 — Environment variables

- `.env.example` (committed) — placeholder password
- `.env` (gitignored) — real password, created locally by whoever clones the repo

## Step 7 — Add the React client

- `client/Dockerfile` — multi-stage: `node:20-alpine` build stage → `nginx:alpine` serve stage, copying the built static files (`/dist` for Vite, `/build` for CRA)
- `client/nginx.conf` — serves the SPA (`try_files ... /index.html`) and reverse-proxies `location /api/ { proxy_pass http://api:8080/; }`
- React app calls relative `/api/...` paths — Nginx forwards internally, so no CORS is needed for the app's own traffic

## Step 8 — Update `docker-compose.yml` to include `client`

- `client` service: builds from `./client`, exposes `3000:80`, depends on `api`
- Both `client` (`3000`) and `api` (`8080`) ports are exposed to the host — client for the app itself, API for direct Swagger/Postman access

## Step 9 — Enable CORS on the API (for direct API access)

Since `api:8080` is now reachable directly (bypassing the Nginx proxy), add a CORS policy in `Program.cs` allowing `http://localhost:3000`, so direct testing tools (Swagger UI, Postman from the browser) and any direct-call code paths work too.

## Step 10 — Build and run

```bash
docker compose up --build
```

Startup sequence:
1. `db` initializes (~15–20s), healthcheck passes
2. `api` starts, connects to `db`, runs pending EF Core migrations automatically
3. `client` (Nginx) starts, serves the React build and proxies `/api/*` to `api`

## Step 11 — Verify

```bash
docker compose ps                # both services healthy/running
docker compose logs -f api       # confirm "Database migrated successfully"
```

- React app: `http://localhost:3000`
- API (Swagger): `http://localhost:8080/swagger`

## Step 12 — Test the "clean clone" experience

Simulate exactly what a recruiter will run:

```bash
docker compose down -v
docker compose up --build
```

If this succeeds with zero manual steps beyond copying `.env.example` to `.env`, the setup is ready to publish.

## Step 13 — Commit and push

```bash
git add Dockerfile client/Dockerfile client/nginx.conf .dockerignore docker-compose.yml .env.example
git commit -m "feat: containerize API, React client, and SQL Server with Docker Compose"
git push origin main
```

## Step 14 — Update README

Document the one-command setup and both exposed URLs:

```markdown
## Run locally
git clone <repo>
cd repo
cp .env.example .env   # then edit SA_PASSWORD
docker compose up --build

- React app: http://localhost:3000
- API (Swagger): http://localhost:8080/swagger

> First run may take ~20–30s while SQL Server initializes and migrations apply automatically — no manual setup needed.
```

---

## Later improvements (optional, worth noting in README as "next steps")

- Separate migration step in CI/CD (EF Core migration bundle) for the Azure production deployment, instead of relying on startup migration there too
- GitHub Actions workflow to build and push both `api` and `client` images and deploy to Azure automatically on push to `main`
- Application Insights for monitoring once deployed to Azure
