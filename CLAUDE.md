# YouScan Test Task — Full-Stack Dashboard

Full spec lives in [REQUIREMENTS.md](REQUIREMENTS.md) — read it before making architecture decisions. This file tracks conventions and commands; update it as the project takes shape (it currently describes the plan, not yet-scaffolded code).

## What this is
A dashboard app with a grid of widgets (line chart, bar chart, text). Widgets are added via a button, laid out 3-per-row, persist across reloads (position + data), and are deletable. Text widgets are editable in place.

## Stack
- **Backend**: .NET 10 Minimal API, **Clean Architecture** (Domain/Application/Infrastructure/Api), **CQRS via MediatR** + FluentValidation, persistence via **SQLite + EF Core**
- **Frontend**: React 18 + TypeScript + Vite, Ant Design, React Query, Recharts
- **Process**: TDD across the full stack — tests written before implementation at every layer
- **Containerization**: Docker + docker-compose locally; deployed to **Azure Container Apps** via Docker images on Docker Hub

Full architecture/sequencing detail lives in the approved plan at `C:\Users\Denys\.claude\plans\cached-puzzling-turtle.md`.

## Structure
```
/backend
  Dashboard.sln, global.json (pinned to .NET 10.0.302)
  src/Dashboard.Domain           entities, enums, domain exceptions — no dependencies
  src/Dashboard.Application      CQRS commands/queries (MediatR), validators (FluentValidation), DTOs — depends on Domain
  src/Dashboard.Infrastructure   EF Core DbContext, repositories, SQLite — depends on Application, Domain
  src/Dashboard.Api              Minimal API endpoints, DI wiring, Dockerfile — depends on Application, Infrastructure
  tests/Dashboard.{Domain,Application,Infrastructure,Api}.Tests   one per src project
/frontend
  Vite + React 18 + TypeScript (strict), Ant Design, React Query, Recharts
  src/test/                      Vitest setup + MSW server (src/test/msw/server.ts)
  src/App.tsx, src/main.tsx      entry points (component structure built out via TDD)
```

## Commands
- Backend build / test (from `/backend`): `dotnet build`, `dotnet test`
- Backend run (from `/backend`): `dotnet run --project src/Dashboard.Api`
- Frontend dev / build / typecheck / test (from `/frontend`, requires Node 24 LTS — `nvm use 24.18.0`): `npm run dev` / `npm run build` / `npm run typecheck` / `npm run test`

## Conventions
- Widget types: `line-chart`, `bar-chart`, `text` — keep a single discriminated shape for widgets on both frontend and backend so new widget types stay easy to add.
- Chart widgets use randomized data (generated server- or client-side — pick one and be consistent).
- Text widget flow: Edit → Save → PUT/PATCH to backend → persisted after reload.
- API must validate input and return proper HTTP status codes (400 for bad input, 404 for missing widget, etc.).
- Frontend must show per-widget loading and error states — don't let one widget's fetch failure break the grid.
- `tsc` must pass with zero errors before calling frontend work done.

## Acceptance criteria (from REQUIREMENTS.md)
- Deployed and publicly testable (Cloudflare/Vercel/etc.)
- Text edits persist through backend
- TypeScript compiles clean
- Errors handled gracefully end-to-end
