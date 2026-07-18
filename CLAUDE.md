# YouScan Test Task — Full-Stack Dashboard

Full spec lives in [REQUIREMENTS.md](REQUIREMENTS.md) — read it before making architecture decisions. This file tracks conventions and commands; update it as the project takes shape (it currently describes the plan, not yet-scaffolded code).

## What this is
A dashboard app with a grid of widgets (line chart, bar chart, text). Widgets are added via a button, laid out 3-per-row, persist across reloads (position + data), and are deletable. Text widgets are editable in place.

## Stack
- **Backend**: .NET Core Web API — persistence via **SQLite + EF Core**
- **Frontend**: React 18 + TypeScript + Vite
- **Charts**: Recharts or Chart.js

## Planned structure
```
/backend    ASP.NET Core Web API project
/frontend   React + TS + Vite app
```
(Update this section once the projects are actually scaffolded — exact folder/project names may differ.)

## Commands
_To be filled in once scaffolded:_
- Backend run / build / test: `dotnet run` / `dotnet build` / `dotnet test` (from `/backend`)
- Frontend dev / build / typecheck: `npm run dev` / `npm run build` / `npm run typecheck` (from `/frontend`)

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
