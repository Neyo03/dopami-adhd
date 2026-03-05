# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the app (development)
cd LifeManager
dotnet run

# Build
dotnet build

# EF Core migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update

# Start PostgreSQL database
docker compose -f LifeManager/compose.yaml up -d lifemanager-db

# Full Docker deployment (db + migrations + app)
docker compose -f LifeManager/compose.yaml up
```

## Architecture

**LifeManager** is an ASP.NET Core 10 Blazor Server app (Interactive Server render mode) for household task management with gamification. The single project is under `LifeManager/`.

## Rules

- Avant de modifier un fichier, tu DOIS lire au moins 3 fichiers similaires pour comprendre le pattern du projet.

### Data Model

- **Home** — top-level entity grouping users, rooms, and tags
- **Room** — belongs to a Home; contains HouseTasks
- **HouseTask** — the core entity; belongs to a Room, has many Tags, optional assigned User, and enums for Duration/Energy/Impact
- **Tag** — belongs to a Home, many-to-many with HouseTask
- **User** — belongs to a Home; holds `TotalXp` for gamification
- **TaskCompletion** — tracks each completion event (who, when, XP earned)

Database: **PostgreSQL** via `Npgsql.EntityFrameworkCore.PostgreSQL`. Connection string is `DefaultConnection` in `appsettings.json`.

### Layer Structure

| Folder              | Purpose                                                                                                                         |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| `Data/`             | EF Core entities and `AppDbContext`                                                                                             |
| `Data/Enums/`       | `TaskDuration`, `TaskEnergy`, `TaskImpact` enums                                                                                |
| `Model/`            | DTOs and form models (never expose EF entities to Razor directly)                                                               |
| `Services/`         | Business logic; use `IDbContextFactory<AppDbContext>` — always `await using var context = await factory.CreateDbContextAsync()` |
| `Extensions/`       | IQueryable extension methods (e.g., `GetRoomsByHome`, `GetTasksByHome`, `GetCompletedTaskByUser`)                               |
| `State/`            | Scoped Blazor state services (e.g., `TagStateService`, `TagModalStateService`) that cache data and fire `OnChange` events       |
| `Components/Pages/` | Blazor pages (`.razor` + optional `.razor.cs` code-behind)                                                                      |
| `Components/`       | Shared Blazor components (`TaskDrawer`, `TagModal`, `RoomTaskList`, `CompletedTaskList`, `RadioGroup`)                          |
| `Migrations/`       | EF Core migration files — do not edit manually                                                                                  |

### Key Patterns

- **Services** are registered as `Scoped` and injected into Blazor pages via `@inject`.
- **State services** (in `State/`) wrap service calls and expose cached lists + `OnChange` event for reactive UI updates.
- **DTOs** (e.g., `TaskDetailsDto`, `RoomDashboardDto`, `UserDto`) are used in service queries instead of raw entities to avoid over-fetching.
- **Bulk EF operations** (`ExecuteUpdateAsync`, `ExecuteDeleteAsync`) are used for single-field updates and deletes to avoid loading entities into memory.
- **Auth**: Cookie-based authentication (`CookieAuth`). The current user is retrieved via `UserService.GetAuthenticatedUserAsync()` which reads from `IHttpContextAccessor`.
- **CSS**: Tailwind CSS + DaisyUI. Styles are generated via the `tailwind/` config and `package.json` scripts.

### Gamification

`LevelingService` calculates user level from `TotalXp` using a threshold array. `UserService.UpdateTotalXpUser` recalculates and persists total XP by summing `TaskCompletion.XpEarned` records.

