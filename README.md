# 📋 TaskFlow — Enterprise Task & Project Management Platform

A high-performance, full-stack Task and Project Management web application built with **Clean Architecture**, **ASP.NET Core Web API**, and **Angular 21 (Zoneless + Signals)**. The platform features an interactive 3-column Kanban board powered by `@angular/cdk/drag-drop` with optimistic UI updates and seamless silent token refresh.

---

## 🚀 Key Highlights & Architectural Features

### 💻 Frontend (Angular v21+)
- **Zoneless Change Detection:** Completely decoupled from `zone.js` (`provideZonelessChangeDetection`), eliminating change-detection overhead and drastically minimising runtime bundle size.
- **Angular Signals:** Fine-grained reactive state management using `signal()`, `computed()`, and signal-based inputs/effects.
- **Interactive Kanban Board:** Built with `@angular/cdk/drag-drop` supporting card moves between columns (Todo, In Progress, Completed), optimistic UI rendering, and instant rollbacks on network failure.
- **Resilient Authentication Pipeline:** Functional Angular HTTP interceptor (`jwtInterceptor`) handling bearer token injection, concurrency-safe silent token refresh on HTTP 401 errors using RxJS `BehaviorSubject` queuing, and auto-logout.
- **Standalone Component Architecture:** 100% standalone components, directives, and functional guards/interceptors with lazy-loaded route chunking.

### ⚙️ Backend (ASP.NET Core)
- **Clean Architecture:** Strict separation of concerns divided into Domain, Application, Infrastructure, and Presentation layers.
- **Entity Framework Core:** Relational database mapping with SQL Server, code-first migrations, and LINQ projection optimisations.
- **Hardened Security & Diagnostics:**
  - JWT Authentication paired with rotating sliding refresh tokens.
  - RFC 7807-compliant `ProblemDetails` global exception handling middleware.
  - IP-based sliding window rate limiting.
  - Database health checks (`/health`).
  - String-based enum serialisation with `JsonStringEnumConverter`.

---

## 🛠️ Tech Stack

| Layer | Technologies |
| :--- | :--- |
| **Frontend** | Angular 21+, TypeScript, Angular Signals, `@angular/cdk/drag-drop`, RxJS, SCSS |
| **Backend** | C#, .NET 8/9, ASP.NET Core Web API, Entity Framework Core |
| **Database** | Microsoft SQL Server / LocalDB |
| **Security** | JWT (JSON Web Tokens), Refresh Tokens, ASP.NET Identity / BCrypt |
| **Documentation & Tooling** | Swagger / OpenAPI, Postman, Vite (Angular Dev Server) |

---

## 📐 Solution Architecture

```text
Task-Manager-Project/
├── backend/
│   ├── TaskManager.Domain/         # Core entities, enums, domain exceptions
│   ├── TaskManager.Application/    # DTOs, interfaces, service contracts, validators
│   ├── TaskManager.Infrastructure/ # EF Core DbContext, migrations, auth services
│   └── TaskManager.API/            # Controllers, middleware, interceptor configs, Program.cs
└── frontend/
    └── src/
        ├── app/
        │   ├── core/               # Interceptors (JWT), guards, services, models
        │   ├── features/           # Projects dashboard, Kanban board (project-detail), Auth
        │   ├── app.config.ts       # Zoneless provider, HTTP interceptors, router config
        │   └── app.routes.ts       # Lazy-loaded routes
        └── environments/           # Environment config (API Base URL)
