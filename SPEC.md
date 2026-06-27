# SPEC.md — ICA Todo Application Technical Specification

## Project Overview

**Application Name:** ICA Todo Application
**Purpose:** Internal task tracking tool for ICA Employees to record and manage daily tasks
**Architecture:** Clean Architecture (shared layers) + Razor Pages frontend + REST API
**Target Runtime:** .NET 8 SDK, SQL Server 2019+

---

## Architecture Overview

```
IcaTodo/
├── src/
│   ├── Core/
│   │   ├── Application/          # Use cases, DTOs, interfaces, services
│   │   └── Domain/               # Entities, domain enums
│   ├── Infrastructure/
│   │   ├── Infrastructure/       # JwtHelper
│   │   └── Persistence/          # EF Core DbContext, repositories, migrations
│   └── Presentation/
│       ├── Todo.API/             # ASP.NET Core 8 REST API (JWT auth)
│       └── Todo.Web/             # ASP.NET Core 8 Razor Pages (cookie auth)
```

Both `Todo.API` and `Todo.Web` share the same Application, Domain, Persistence, and Infrastructure layers.

---

## Backend Specification

### Technology Stack

| Component | Technology |
|---|---|
| Framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`) |
| API Auth | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Web Auth | Cookie (`Microsoft.AspNetCore.Authentication.Cookies`) |
| Password Hashing | BCrypt.Net-Next |
| Architecture | Clean Architecture |
| Patterns | Repository Pattern, Service Layer, Dependency Injection |

### NuGet Packages

```xml
<!-- Domain -->
(no external packages)

<!-- Application -->
BCrypt.Net-Next (4.x)

<!-- Persistence -->
Microsoft.EntityFrameworkCore (8.x)
Microsoft.EntityFrameworkCore.SqlServer (8.x)
Microsoft.EntityFrameworkCore.Design (8.x)

<!-- Infrastructure -->
System.IdentityModel.Tokens.Jwt (7.x)
Microsoft.AspNetCore.Authentication.JwtBearer (8.x)

<!-- Todo.API -->
Microsoft.AspNetCore.Authentication.JwtBearer (8.x)
Swashbuckle.AspNetCore (6.x)

<!-- Todo.Web -->
(uses framework packages only — no additional NuGet)
```

### Project Structure

```
src/
├── Core/
│   ├── Application/
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   │   ├── RegisterRequestDto.cs
│   │   │   │   ├── LoginRequestDto.cs
│   │   │   │   └── LoginResponseDto.cs
│   │   │   ├── Todo/
│   │   │   │   ├── CreateTaskDto.cs
│   │   │   │   ├── TaskResponseDto.cs
│   │   │   │   ├── UpdateTaskStatusDto.cs
│   │   │   │   ├── PaginatedResultDto.cs
│   │   │   │   ├── DashboardDto.cs
│   │   │   │   └── ProjectSummaryDto.cs
│   │   │   └── Project/
│   │   │       ├── CreateProjectDto.cs
│   │   │       ├── UpdateProjectDto.cs
│   │   │       └── ProjectResponseDto.cs
│   │   ├── RepositoryInterfaces/
│   │   │   ├── IUserRepository.cs
│   │   │   ├── ITaskRepository.cs
│   │   │   └── IProjectRepository.cs
│   │   ├── ServiceInterfaces/
│   │   │   ├── IAuthService.cs
│   │   │   ├── ITaskService.cs
│   │   │   ├── IProjectService.cs
│   │   │   └── IJwtHelper.cs
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── TaskService.cs
│   │   │   └── ProjectService.cs
│   │   └── Wrappers/
│   │       └── ApiResponse.cs
│   └── Domain/
│       └── Entities/
│           ├── User.cs
│           ├── TaskItem.cs         # includes TaskPriority enum
│           └── Project.cs
├── Infrastructure/
│   ├── Infrastructure/
│   │   └── Helpers/
│   │       └── JwtHelper.cs
│   └── Persistence/
│       ├── Context/
│       │   └── AppDbContext.cs
│       ├── Repositories/
│       │   ├── UserRepository.cs
│       │   ├── TaskRepository.cs
│       │   └── ProjectRepository.cs
│       └── Migrations/
└── Presentation/
    ├── Todo.API/
    │   ├── Controllers/
    │   │   ├── AuthController.cs
    │   │   └── TaskController.cs
    │   ├── Middleware/
    │   │   └── ExceptionMiddleware.cs
    │   ├── Program.cs
    │   └── appsettings.json
    └── Todo.Web/
        ├── Pages/
        │   ├── Shared/
        │   │   ├── _Layout.cshtml
        │   │   ├── _ViewImports.cshtml
        │   │   └── _ViewStart.cshtml
        │   ├── Auth/
        │   │   ├── Login.cshtml + Login.cshtml.cs
        │   │   ├── Register.cshtml + Register.cshtml.cs
        │   │   └── Logout.cshtml + Logout.cshtml.cs
        │   ├── Dashboard/
        │   │   └── Index.cshtml + Index.cshtml.cs
        │   ├── Tasks/
        │   │   └── Index.cshtml + Index.cshtml.cs
        │   ├── Projects/
        │   │   └── Index.cshtml + Index.cshtml.cs
        │   ├── Index.cshtml + Index.cshtml.cs    ← redirects to Dashboard
        │   └── Error.cshtml + Error.cshtml.cs
        ├── wwwroot/
        │   ├── css/site.css
        │   └── js/site.js
        ├── Program.cs
        └── appsettings.json
```

---

## Domain Entities

### User (TBL_User)

```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }           // NOT NULL, nvarchar(100)
    public string Email { get; set; }              // NOT NULL, UNIQUE, nvarchar(255)
    public string PasswordHash { get; set; }       // NOT NULL, nvarchar(255) BCrypt hash
    public DateTime CreatedDate { get; set; }      // DEFAULT UTC_NOW
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<TaskItem> Tasks { get; set; }
    public ICollection<Project> Projects { get; set; }
}
```

### Project (TBL_Project)

```csharp
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }               // NOT NULL, nvarchar(100)
    public string? Description { get; set; }       // nvarchar(500)
    public string Color { get; set; } = "#003087"; // hex color for UI badge
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public ICollection<TaskItem> Tasks { get; set; }
}
```

### TaskItem (TBL_Task)

```csharp
public enum TaskPriority { Low = 0, Medium = 1, High = 2, Critical = 3 }

public class TaskItem
{
    public int Id { get; set; }
    public string Task { get; set; }               // NOT NULL, nvarchar(500)
    public string? Description { get; set; }       // nvarchar(1000)
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int? ProjectId { get; set; }            // nullable FK -> TBL_Project
    public Project? Project { get; set; }
}
```

---

## API Endpoints (Todo.API — JWT auth)

### Auth Controller — `/api/auth`

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| POST | `/api/auth/register` | Register new user | No |
| POST | `/api/auth/login` | Login and get JWT | No |

### Task Controller — `/api/tasks`

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| GET | `/api/tasks` | Get paginated tasks | Yes |
| GET | `/api/tasks/search?q={query}&page={n}` | Search tasks | Yes |
| POST | `/api/tasks` | Create new task | Yes |
| PATCH | `/api/tasks/{id}/status` | Update task completion | Yes |
| DELETE | `/api/tasks/{id}` | Soft delete task | Yes |

---

## Razor Pages Routes (Todo.Web — cookie auth)

| URL | Page | Auth Required |
|---|---|---|
| `/` | `Pages/Index` → redirect to Dashboard | No (redirect) |
| `/Auth/Login` | `Pages/Auth/Login` | No |
| `/Auth/Register` | `Pages/Auth/Register` | No |
| `/Auth/Logout` | `Pages/Auth/Logout` | Yes |
| `/Dashboard` | `Pages/Dashboard/Index` | Yes |
| `/Tasks` | `Pages/Tasks/Index` | Yes |
| `/Projects` | `Pages/Projects/Index` | Yes |

### PageModel Handler Methods (Tasks page example)

| Handler | Trigger | Action |
|---|---|---|
| `OnGetAsync` | GET `/Tasks` | Load tasks + projects |
| `OnPostCreateAsync` | POST with `handler=Create` | Create new task |
| `OnPostToggleAsync` | POST with `handler=Toggle` | Toggle completion |
| `OnPostDeleteAsync` | POST with `handler=Delete` | Soft delete |

---

## API Response Wrapper

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") => ...;
    public static ApiResponse<T> Fail(string message, List<string>? errors = null) => ...;
}
```

---

## DTOs

### TaskResponseDto

```csharp
public class TaskResponseDto
{
    public int Id { get; set; }
    public string Task { get; set; }
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public string PriorityLabel { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? ProjectColor { get; set; }
}
```

### DashboardDto

```csharp
public class DashboardDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int RemainingTasks { get; set; }
    public int CompletedToday { get; set; }
    public double CompletedTodayPercentage { get; set; }
    public int OverdueTasks { get; set; }
    public int HighPriorityTasks { get; set; }
    public int TotalProjects { get; set; }
    public List<ProjectSummaryDto> ProjectSummaries { get; set; }
}
```

---

## Validation Rules

**Register:**
- Username: Required, 2–100 chars
- Email: Required, valid email format, unique
- Password: Required, min 8 chars, at least one number and one uppercase letter
- ConfirmPassword: Must match Password

**Login:**
- Email: Required, valid email format
- Password: Required

**Create Task:**
- Task: Required, 1–500 chars
- Description: Optional, max 1000 chars
- Priority: Required enum value
- DueDate: Optional, must not be in the past

**Create Project:**
- Name: Required, 1–100 chars
- Description: Optional, max 500 chars
- Color: Optional hex color, defaults to `#003087`

---

## Pagination

Default page size: **10 tasks per page**

```json
{
  "success": true,
  "message": "Tasks retrieved",
  "data": {
    "items": [...],
    "totalCount": 45,
    "page": 1,
    "pageSize": 10,
    "totalPages": 5
  }
}
```

---

## Database Schema (EF Core Code-First — SQL Server)

### Connection String Format

```
Server=<host>,1433;Database=<dbname>;User Id=<user>;Password=<pass>;TrustServerCertificate=True;Encrypt=False;
```

### SQL Server Tables

```sql
-- TBL_User
CREATE TABLE [TBL_User] (
  [Id]           INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Username]     NVARCHAR(100) NOT NULL,
  [Email]        NVARCHAR(255) NOT NULL,
  [PasswordHash] NVARCHAR(255) NOT NULL,
  [CreatedDate]  DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  [UpdatedDate]  DATETIME2 NULL,
  [IsActive]     BIT NOT NULL DEFAULT 1,
  CONSTRAINT UQ_TBL_User_Email UNIQUE ([Email])
);

-- TBL_Project
CREATE TABLE [TBL_Project] (
  [Id]          INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Name]        NVARCHAR(100) NOT NULL,
  [Description] NVARCHAR(500) NULL,
  [Color]       NVARCHAR(7) NOT NULL DEFAULT '#003087',
  [IsDeleted]   BIT NOT NULL DEFAULT 0,
  [CreatedDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  [UpdatedDate] DATETIME2 NULL,
  [UserId]      INT NOT NULL REFERENCES [TBL_User]([Id]) ON DELETE NO ACTION
);

-- TBL_Task
CREATE TABLE [TBL_Task] (
  [Id]            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  [Task]          NVARCHAR(500) NOT NULL,
  [Description]   NVARCHAR(1000) NULL,
  [Priority]      INT NOT NULL DEFAULT 1,
  [DueDate]       DATETIME2 NULL,
  [IsCompleted]   BIT NOT NULL DEFAULT 0,
  [IsDeleted]     BIT NOT NULL DEFAULT 0,
  [CreatedDate]   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  [UpdatedDate]   DATETIME2 NULL,
  [CompletedDate] DATETIME2 NULL,
  [UserId]        INT NOT NULL REFERENCES [TBL_User]([Id]) ON DELETE CASCADE,
  [ProjectId]     INT NULL REFERENCES [TBL_Project]([Id]) ON DELETE SET NULL
);
```

---

## Security Requirements

- Passwords hashed with BCrypt (work factor: 12)
- Cookie auth expires in 24 hours (sliding expiration enabled)
- JWT tokens expire in 24 hours (for Todo.API consumers)
- All Razor Pages decorated with `[Authorize]` except Login/Register
- All Task/Project API endpoints protected by `[Authorize]`
- UserId extracted from claims — never trusted from form fields or request body
- Soft delete (`IsDeleted = true`) — no hard deletes on tasks or projects
- Cookie: `HttpOnly = true`, `SecurePolicy = SameAsRequest`

---

## Environment Setup

### Todo.Web (Razor Pages frontend)

```bash
cd src/Presentation/Todo.Web
dotnet restore
dotnet run
# App runs on https://localhost:7002 (or configured port)
```

### Todo.API (REST API)

```bash
cd src/Presentation/Todo.API
dotnet restore
dotnet run
# API runs on https://localhost:7001
# Swagger: https://localhost:7001/swagger
```

### EF Core Migrations

```bash
# Run from solution root — use Todo.Web as startup project
dotnet ef migrations add InitialCreate \
  --project src/Infrastructure/Persistence \
  --startup-project src/Presentation/Todo.Web

dotnet ef database update \
  --project src/Infrastructure/Persistence \
  --startup-project src/Presentation/Todo.Web
```
