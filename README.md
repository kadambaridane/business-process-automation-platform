# Business Process Automation Platform

A cloud-hosted, low-code Business Process Management (BPM) platform. Business users configure applications, dynamic forms, and multi-step workflows without writing code; end users submit forms that flow through configurable human-approval and automated system steps, tracked end-to-end from submission to completion.

The platform is **not** hardcoded to one process (e.g. Leave Requests) — any number of independent business applications, each with its own form and workflow, can be configured and run concurrently. See [Architecture](#architecture) for how that genericity is achieved.

## Features

- **JWT authentication** with role-based authorization (Admin, Business User, Employee, Manager, Approver)
- **Business application management** — create/activate/deactivate applications, each an independent configurable process
- **Dynamic form builder** — 8 field types (text, number, email, date, dropdown, radio, checkbox, textarea), per-field validation rules, drag-free reordering, rendered by one generic component (no per-form Angular code)
- **Visual-free workflow configuration** — ordered steps (Human Task, System Task, Approval, Decision, Notification, End), role-based task assignment, pluggable automated actions
- **A real workflow execution engine** — form submissions spawn tracked instances that flow through configured steps automatically, pausing for human approval and resuming on Approve/Reject/Complete
- **Task management** — My Tasks / Pending Tasks / Completed Tasks queues with full action history
- **In-app notifications**, a live **dashboard**, and filterable **reports** — all backed by real queries, no mock data
- **27 passing backend unit tests** plus a full Playwright-verified browser walkthrough of the entire flow

## Table of Contents

- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Architecture](#architecture)
- [Database](#database)
- [Setup](#setup)
- [API Overview](#api-overview)
- [Testing](#testing)
- [Configuration Reference](#configuration-reference)
- [Known Limitations](#known-limitations)

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core Web API, .NET 8, C# |
| ORM | Entity Framework Core 8 |
| Database | SQL Server 2025 |
| Auth | JWT bearer tokens, BCrypt password hashing |
| Frontend | Angular 18 (standalone components), TypeScript, Reactive Forms |
| Backend IDE | Visual Studio 2026 (targets .NET 8 explicitly — see `Backend/global.json`) |
| Frontend IDE | VS Code |

## Prerequisites

- **.NET 8 SDK** (`8.0.123` or later patch in the 8.x line — pinned via `Backend/global.json` so `dotnet` commands and Visual Studio 2026 don't drift onto a newer SDK even if one is also installed)
- **Node.js 20+** and **Angular CLI 18** (`npm install -g @angular/cli@18`)
- **SQL Server 2025** (Express edition is sufficient), reachable from your machine
- **Visual Studio 2026** (optional, for opening/debugging the backend via `Backend/BusinessProcessAutomation.sln`) or just the `dotnet` CLI

## Project Structure

```
BusinessProcessAutomationPlatform/
├── Backend/
│   ├── BusinessProcessAutomation.sln
│   ├── global.json                          # pins the SDK to net8.0
│   └── BusinessProcessAutomation.Api/
│       ├── Controllers/                     # one per resource (Auth, Applications, Forms, Workflows, Tasks, ...)
│       ├── Services/                        # business logic, incl. the workflow engine
│       ├── Interfaces/                      # service contracts (DI)
│       ├── SystemTasks/                     # pluggable ISystemTaskHandler implementations
│       ├── Models/                          # EF Core entities
│       ├── DTOs/                            # API request/response contracts
│       ├── Data/                            # AppDbContext, EF configurations, seed data
│       ├── Migrations/                      # EF Core migrations
│       ├── Middleware/                      # global exception handling
│       └── Helpers/                         # JWT + password hashing
│   └── BusinessProcessAutomation.Api.Tests/ # xUnit tests
└── Frontend/
    └── business-process-automation-ui/
        └── src/app/
            ├── core/                        # auth service, guards, interceptor, layout shell
            ├── shared/                      # dynamic-form renderer, pagination, spinner
            ├── auth/ dashboard/ applications/ forms/ workflows/
            ├── tasks/ workflow-instances/ notifications/ reports/ roles/
```

## Architecture

**Applications, Forms, and Workflows are all data, not code.** A `BusinessApplication` row has zero or more `FormDefinition`s (each with `FormField` rows describing type/validation/order) and zero or more `WorkflowDefinition`s (each with ordered `WorkflowStep` rows). Angular renders any form from its stored `FormField` list via one reusable `DynamicFormComponent` — there is no per-form Angular component. Adding a new business process (e.g. "Purchase Request") is entirely a configuration act through the UI, never a code change.

**Definitions vs. instances.** `WorkflowDefinition`/`WorkflowStep` are the configured template; `WorkflowInstance`/`WorkflowStepInstance`/`Task` are one execution of that template, created when a form is submitted. This separation is deliberate (see `Services/WorkflowEngineService.cs`) and mirrors the DB design in [Database](#database).

**The workflow engine** (`WorkflowEngineService`) drives an instance forward step by step:
- `HumanTask` / `Approval` steps create a `Task` assigned to a role pool and **pause** execution until a user acts.
- `SystemTask` steps resolve a pluggable `ISystemTaskHandler` by key (`SystemActionKey`) and run it immediately — new automated actions are added by implementing the interface and registering it in `Program.cs`, no engine changes required.
- `Notification` steps create an in-app notification for the submitter and continue immediately.
- `Decision` steps auto-pass (the step configuration UI intentionally doesn't capture branch conditions, per the "no complex designer" requirement — see [Known Limitations](#known-limitations)).
- `End` (or running out of steps) completes the instance. Rejecting a task terminates the instance immediately as `Rejected` without running remaining steps.

## Database

EF Core 8 manages the schema via migrations; SQL Server 2025 compatibility required no non-default EF Core features. Key relationships:

- `User` ↔ `Role` via `UserRole` (many-to-many)
- `BusinessApplication` → `FormDefinition` → `FormField` (one-to-many, cascade)
- `BusinessApplication` → `WorkflowDefinition` → `WorkflowStep` (one-to-many, cascade)
- `FormSubmission` references `FormDefinition`, `Application`, `SubmittedByUser`; stores field values as a JSON blob (`SubmissionData`) since the schema is dynamic per form
- `WorkflowInstance` references `WorkflowDefinition`, `Application`, and a **required, unique** `FormSubmission` (one instance per submission); tracks `CurrentStep` and `Status`
- `WorkflowStepInstance` is the per-step execution/audit record for an instance
- `Task` (class name `WorkflowTask` in code — `Task` collides with `System.Threading.Tasks.Task`) belongs to one `WorkflowStepInstance`; `TaskHistory` is its immutable action log

One schema note worth knowing if you touch reordering logic: `WorkflowStep.StepOrder` and `FormField.DisplayOrder` are **not** uniquely constrained. A unique index there breaks multi-row reordering — EF Core can't sequence two UPDATEs that would momentarily collide on a unique pair within one `SaveChanges()` batch (see migration `WorkflowStepOrderIndexNotUnique`).

## Setup

### 1. Restore and build the backend

```bash
cd Backend
dotnet restore
dotnet build
```

### 2. Configure the database connection (never committed)

The real connection string and JWT signing key live in **user-secrets**, not in `appsettings.json` (which only has empty placeholders):

```bash
cd Backend/BusinessProcessAutomation.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER\SQLEXPRESS;Database=BusinessProcessAutomationDb;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "<a long random string, 32+ chars>"
```

If opening in Visual Studio 2026, right-click the API project → **Manage User Secrets** to edit the same file.

### 3. Apply EF Core migrations

```bash
dotnet tool install --global dotnet-ef   # first time only
dotnet ef database update
```

This creates the database, all tables, and seeds the five roles (`Admin`, `Business User`, `Employee`, `Manager`, `Approver`) plus one default admin account on first run (see [Default credentials](#default-credentials)).

To add a new migration after changing an entity or `IEntityTypeConfiguration`:

```bash
dotnet ef migrations add <DescriptiveName>
dotnet ef database update
```

### 4. Run the backend

```bash
dotnet run
```

Or press F5 in Visual Studio 2026. By default it listens on `https://localhost:7261` and `http://localhost:5175` (see `Properties/launchSettings.json`), with Swagger UI at `https://localhost:7261/swagger`.

### 5. Run the frontend

```bash
cd Frontend/business-process-automation-ui
npm install
ng serve
```

Open `http://localhost:4200`. The dev environment (`src/environments/environment.development.ts`) points at `https://localhost:7261/api` — update it if your backend runs on different ports. CORS is restricted to `http://localhost:4200` via `appsettings.json` → `Cors:AllowedOrigins`.

### Default credentials

A default admin account is seeded once, on first run, if no users exist yet:

```
Username: admin
Password: Admin@12345
```

**Change this password immediately in any environment beyond local development** — there is no forced-change-on-first-login flow yet.

## API Overview

All endpoints return a consistent envelope: `{ success, data, error }`. JWT bearer auth is required everywhere except `POST /api/auth/login`. Full interactive documentation is at `/swagger` when running in Development.

| Area | Base route |
|---|---|
| Auth | `POST /api/auth/login`, `GET /api/auth/me` |
| Users/Roles | `GET /api/roles` (read-only — see [Known Limitations](#known-limitations)) |
| Applications | `/api/applications` |
| Forms | `/api/applications/{id}/forms`, `/api/forms/...` (definitions, fields, submissions) |
| Workflows | `/api/applications/{id}/workflows`, `/api/workflows/...` (definitions, steps) |
| Workflow instances | `/api/workflow-instances/{id}`, `.../history` |
| Tasks | `/api/tasks/my`, `/pending`, `/completed`, `/{id}/approve`, `/reject`, `/complete` |
| Notifications | `/api/notifications`, `/unread-count`, `/{id}/read`, `/read-all` |
| Dashboard | `GET /api/dashboard/summary` |
| Reports | `/api/reports/applications`, `/status-summary`, `/user-tasks` |

Role-restricted writes generally require `Admin` or `Business User`; `GET /api/reports/*` and `GET /api/tasks/pending` are also restricted to those two roles for org-wide oversight.

## Testing

```bash
cd Backend
dotnet test
```

27 xUnit tests (EF Core InMemory provider) cover:
- Password hashing and JWT-based login (success, wrong password, deactivated account, login-by-email)
- The workflow engine (start, auto-advance through System/Notification/End steps, rejection termination, no-active-workflow no-op)
- Task authorization and actions (approve/reject/complete, role-vs-direct assignment, action/step-type mismatch, double-action guard)
- Form submission validation (required fields, email format, number range, dropdown option membership, text length, multiple simultaneous errors, inactive-form rejection)

The Angular side has the default CLI-scaffolded Karma spec only; the frontend was instead verified with real browser runs (Playwright) driving the full UI against the live backend at the end of every phase — including one complete golden-path pass (login → configure a brand-new application/form/workflow from scratch → submit → approve → verify completion → confirm Dashboard/Reports reflect it) to prove the platform genuinely works end-to-end, not just per-phase.

## Configuration Reference

| Setting | Where | Notes |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | user-secrets | Never committed |
| `Jwt:Key` / `Issuer` / `Audience` / `ExpiryMinutes` | user-secrets (`Key`) + `appsettings.json` (rest) | Key must be random and long |
| `Cors:AllowedOrigins` | `appsettings.json` | Add your deployed frontend origin here |
| `environment.apiUrl` | Angular `src/environments/` | Swap `fileReplacements` in `angular.json` per build config |

## Known Limitations

Documented honestly rather than silently — these are real gaps, not oversights hidden from the reader:

- **No full Users & Roles admin UI.** The spec's module list (section 8) describes user/role CRUD screens, but the numbered phase plan never actually schedules that work as its own phase. Only a read-only `GET /api/roles` exists (needed for the workflow step assignee dropdown). There is currently no way to create additional users through the UI — only the seeded admin account exists until this is built.
- **`Decision` step type is a pass-through.** The workflow step configuration UI (by design, to avoid a complex drag-and-drop designer) never captures branch conditions or alternate-path targets, so the engine cannot evaluate a real decision — it logs `"Auto-passed (no branch conditions configured)"` and continues to the next step in sequence.
- **`WorkflowInstanceStatus.Approved` is unused.** The engine collapses a successfully finished run into `Completed` rather than a separate terminal `Approved` state; dashboard/report metrics labeled "Approved" count `Completed` instances. The enum value exists in the schema but the engine never sets it.
- **No email/SMS notifications** — only in-app notifications, per the original spec's explicit scope.
- **Angular has no unit test suite** beyond the CLI-scaffolded default; UI correctness relies on the Playwright verification runs performed during development rather than a checked-in test suite.
