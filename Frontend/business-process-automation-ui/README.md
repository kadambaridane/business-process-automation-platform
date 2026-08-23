# Business Process Automation Platform — Frontend

Angular 18 frontend for the Business Process Automation Platform. See the [root README](../../README.md) for full architecture, setup, and API documentation.

## Quick start

```bash
npm install
ng serve
```

Open `http://localhost:4200`. Requires the backend API running (see `../../Backend/`) — the dev environment points at `https://localhost:7261/api` by default (`src/environments/environment.development.ts`).

## Build

```bash
ng build                                  # production, outputs to dist/
ng build --configuration development      # dev build
```
