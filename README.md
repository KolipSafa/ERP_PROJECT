# ERP Project – Setup & Deployment Guide (EN)

Modern ERP sample using .NET 9 (Clean Architecture) + Vue 3 + Supabase (Postgres + Auth).

## Tech Stack
- Backend: .NET 9, ASP.NET Core, EF Core (Npgsql), MediatR, AutoMapper, Serilog
- Frontend: Vue 3, Vite, TypeScript, Pinia, Vuetify
- Infra: Supabase (Postgres, Auth, Edge Functions), Render (API), Vercel (Web)

## Repository Structure
- `src/Backend/` – API (`API.Web`), Application, Domain, Infrastructure
- `src/Frontend/client-app/` – Vue app
- `supabase/` – Edge functions and config

## Prerequisites
- .NET 9 SDK
- Node.js LTS (18+)
- Supabase project (URL + keys)

## Local Setup

### Backend (.NET)
1) Restore & build
```bash
dotnet restore
dotnet build src/Backend/API.Web/API.Web.csproj
```

2) Create local config from example
```bash
cp src/Backend/API.Web/appsettings.Development.json.example src/Backend/API.Web/appsettings.Development.json
```
Fill placeholders (Postgres connection string, Supabase URL/keys). See Environment variables section.

3) Apply migrations (Postgres)
```bash
dotnet ef database update \
  --project src/Backend/Infrastructure \
  --startup-project src/Backend/API.Web
```

4) Run API
```bash
dotnet run --project src/Backend/API.Web/API.Web.csproj
```
API will listen at `https://localhost:7277` and `http://localhost:5245`.

### Frontend (Vue)
```bash
cd src/Frontend/client-app
npm install
npm run dev
```
Vite dev server runs at `http://localhost:5173`.

## Environment Variables (Public repo – never commit secrets)

Backend (use environment variables in hosting or local user-secrets)
- `ConnectionStrings__DefaultConnection` – Postgres conn string (e.g. Supabase Session Pooler)
- `Supabase__Url` – `https://<PROJECT-REF>.supabase.co`
- `Supabase__ServiceRoleKey` – service_role key (backend only)
- `Supabase__AnonKey` – anon key (optional)
- `Jwt__Authority` – `https://<PROJECT-REF>.supabase.co/auth/v1`
- `Jwt__Audience` – `authenticated`

Frontend (`src/Frontend/client-app/.env`, gitignored)
- `VITE_SUPABASE_URL`
- `VITE_SUPABASE_ANON_KEY`
- `VITE_API_BASE_URL` – backend API base

## Supabase Setup (Quick)
1) Create project → copy Project URL, Anon key, Service Role key
2) Postgres: use Session Pooler connection string for the API
3) Auth: Email provider enabled (default)
4) (Optional) Deploy Edge Function `invite-user` in `supabase/functions/`

## Deployment

### Backend on Render
- Environment: .NET 9
- Build Command:
```bash
dotnet publish src/Backend/API.Web/API.Web.csproj -c Release -o out
```
- Start Command:
```bash
dotnet out/API.Web.dll
```
- Environment Variables (Render Dashboard):
  - All Backend variables listed above

### Frontend on Vercel
- Framework preset: Vite
- Build Command: `npm run build`
- Output Directory: `dist`
- Environment Variables:
  - `VITE_SUPABASE_URL`, `VITE_SUPABASE_ANON_KEY`, `VITE_API_BASE_URL`

### Database/Auth (Supabase)
- Nothing to build. Ensure RLS/Policies are as required (this project authorizes in API).

## Security Checklist
- Do NOT commit secrets. Use env vars.
- Swagger, request-claim logging, Hangfire dashboard are enabled only in Development.
- CORS only allows your frontend origins.
- JWT validation uses JWKS or legacy secret; admin operations use Service Role key via backend only.

## Troubleshooting
- Migrations: ensure Postgres connection string (host, port, db, user, password, SSL Mode) is correct.
- 401s: verify frontend sends Supabase access token; check API `Jwt:Authority/Audience`.
- CORS: add your deployed frontend origin to CORS in `Program.cs`.

