# Widget Dashboard

A dashboard where users add line-chart, bar-chart, and text widgets to a 3-per-row grid. Widget position and data persist across reloads, text widgets are editable in place, and the grid supports infinite scroll with a back-to-top control for long boards.

## Live demo

- **Frontend**: https://ambitious-rock-0a1432a03.7.azurestaticapps.net
- **Backend API**: https://ca-youscan-backend.wonderfulsky-57adee35.westeurope.azurecontainerapps.io/api/widgets

The backend runs on Azure Container Apps with `min replicas: 0`, so the very first request after a period of inactivity may take a few seconds while it cold-starts.

## Stack

- **Backend**: .NET 10 Minimal API, Clean Architecture (Domain / Application / Infrastructure / Api), CQRS via MediatR + FluentValidation, AutoMapper for entity→DTO mapping, EF Core + SQLite for persistence
- **Frontend**: React 18 + TypeScript (strict) + Vite, Ant Design, React Query, Recharts
- **Testing**: TDD across the full stack — xUnit for the backend (one test project per layer), Vitest + React Testing Library + MSW for the frontend
- **Containerization**: Docker + docker-compose locally
- **Deployment**: Azure Container Apps (backend, Docker image from Docker Hub) + Azure Static Web Apps (frontend), wired up via GitHub Actions

## Features

- Add line-chart, bar-chart, and text widgets to a 3-per-row grid via a button
- Widget position and data persist across reloads
- Text widgets are editable in place: Edit → Save → persisted through the backend
- Widgets are deletable, with a confirmation prompt
- Per-widget loading and error states — one widget's fetch failure doesn't break the rest of the grid
- Cursor-paginated, infinite-scrolling grid, with a back-to-top button that appears once the page has scrolled past the fold

## Design notes

- The grid renders widgets as a flat ordered sequence (3 per row via the layout); the `row`/`column` fields on the API's widget DTO are derived, informational values computed from the widget's order — the frontend does not position widgets by them.
- The API self-documents via OpenAPI at `/openapi/v1.json` (enabled in all environments, including the deployed backend).

## Project structure

```
/backend
  src/Dashboard.Domain           entities, enums, domain exceptions — no dependencies
  src/Dashboard.Application      CQRS commands/queries (MediatR), FluentValidation validators, DTOs
  src/Dashboard.Infrastructure   EF Core DbContext, repositories, SQLite
  src/Dashboard.Api              Minimal API endpoints, DI wiring, Dockerfile
  tests/Dashboard.{Domain,Application,Infrastructure,Api}.Tests
/frontend
  src/api/                       API client + typed fetch functions
  src/components/                Dashboard, WidgetGrid/WidgetItem, chart + text widget components
  src/test/                      Vitest setup + MSW handlers
.github/workflows/               CI/CD (backend.yml, frontend.yml)
docker-compose.yml                local full-stack run
```

## Running locally

```
docker compose up --build
```

- Frontend: http://localhost:5173
- Backend: http://localhost:8080/api/widgets

Widget data persists in a named Docker volume (`sqlite-data`) mounted at `/data` in the backend container, so it survives container restarts.

### Running without Docker

Backend (requires .NET SDK 10.0.302, pinned via `global.json`):
```
cd backend
dotnet run --project src/Dashboard.Api
```

Frontend (requires Node 24 LTS):
```
cd frontend
npm install
npm run dev
```

## Running tests

Backend:
```
cd backend
dotnet test
```

Frontend:
```
cd frontend
npm run typecheck
npm run lint
npm run test
```

## Deployment

### Infrastructure

- **Backend**: Azure Container Apps, `westeurope`, scale-to-zero (min 0 / max 1 replica). SQLite persists on an Azure Files share mounted at `/data` — this is set via the `ConnectionStrings__Default` environment variable on the Container App (`Data Source=/data/dashboard.db`); without it, EF Core falls back to the container's own ephemeral filesystem and all data is lost on every restart or scale-to-zero cycle.
- **Frontend**: Azure Static Web Apps (Free tier), `westeurope`.
- **CORS**: the backend's allowed origin is set via the `Cors__AllowedOrigins__0` environment variable, pointed at the Static Web App's hostname.

### CI/CD

Two GitHub Actions workflows, triggered on push to `master` (path-filtered) and manually via `workflow_dispatch`:

- **`.github/workflows/backend.yml`**: `dotnet test` → build & push the Docker image to Docker Hub → deploy to Container Apps. Azure authentication uses **OIDC** (`azure/login@v2` with a federated credential on an Azure AD app registration) — no long-lived Azure secret is stored in GitHub at all.
- **`.github/workflows/frontend.yml`**: `npm run typecheck && npm run lint && npm run test && npm run build` → deploy to Static Web Apps via its official deploy action.

Required GitHub repo secrets:

| Secret | Purpose |
|---|---|
| `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN` | push the backend image |
| `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` | OIDC login to Azure (identifiers, not secrets, but stored as such for convenience) |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | deploy the frontend |

Note on GitHub OIDC subjects: GitHub now issues immutable-ID subject claims (`repo:OWNER@ORG_ID/REPO@REPO_ID:ref:...`) rather than the plain `repo:OWNER/REPO:ref:...` format shown in most docs — if you're setting this up fresh on a different repo, check the actual subject claim in a failed run's logs before creating the federated credential, rather than assuming the plain format.
