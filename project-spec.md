# project-spec.md — ICA Todo Application: Complete Build Prompt

## Master Prompt for Claude Code

You are a Senior Full Stack Software Architect and Senior .NET Developer. Build a complete, production-ready **ICA Employee Todo Application** from scratch using the specifications below. Every file must be fully implemented — no stubs, no placeholders.

---

## What to Build

A full-stack internal task management application for ICA Employees consisting of:

1. **ASP.NET Core 8 Razor Pages Web App** (`Todo.Web`) — Bootstrap 5.3, Cookie auth, dark/light theme
2. **ASP.NET Core 8 REST API** (`Todo.API`) — Clean Architecture, JWT auth, Swagger
3. **Shared Layers** — Domain, Application, Persistence, Infrastructure (used by both presentations)
4. **SQL Server Database** — Code-first schema via EF Core migrations

---

## Solution Structure

Create a Visual Studio solution file `IcaTodo.sln` at the root with these projects:

```
IcaTodo/
├── IcaTodo.sln
├── CLAUDE.md
├── SPEC.md
├── project-spec.md
└── src/
    ├── Core/
    │   ├── Application/
    │   │   ├── Application.csproj
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
    │       ├── Domain.csproj
    │       └── Entities/
    │           ├── User.cs
    │           ├── Project.cs
    │           └── TaskItem.cs        ← includes TaskPriority enum
    ├── Infrastructure/
    │   ├── Infrastructure/
    │   │   ├── Infrastructure.csproj
    │   │   └── Helpers/
    │   │       └── JwtHelper.cs
    │   └── Persistence/
    │       ├── Persistence.csproj
    │       ├── Context/
    │       │   └── AppDbContext.cs
    │       └── Repositories/
    │           ├── UserRepository.cs
    │           ├── TaskRepository.cs
    │           └── ProjectRepository.cs
    └── Presentation/
        ├── Todo.API/
        │   ├── Todo.API.csproj
        │   ├── Program.cs
        │   ├── appsettings.json
        │   ├── Controllers/
        │   │   ├── AuthController.cs
        │   │   └── TaskController.cs
        │   └── Middleware/
        │       └── ExceptionMiddleware.cs
        └── Todo.Web/                          ← ASP.NET Core 8 Razor Pages
            ├── Todo.Web.csproj
            ├── Program.cs
            ├── appsettings.json
            ├── web.config
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
            │   ├── Index.cshtml + Index.cshtml.cs
            │   └── Error.cshtml + Error.cshtml.cs
            └── wwwroot/
                ├── css/site.css
                └── js/site.js
```

---

## Step-by-Step Implementation Instructions

### STEP 1: Domain Layer (`src/Core/Domain`)

Create `Domain.csproj` targeting `net8.0`.

**`User.cs`** — Properties: `Id (int PK)`, `Username (string)`, `Email (string)`, `PasswordHash (string)`, `CreatedDate (DateTime)`, `UpdatedDate (DateTime?)`, `IsActive (bool, default true)`, navigation: `ICollection<TaskItem> Tasks`, `ICollection<Project> Projects`.

**`Project.cs`** — Properties: `Id (int PK)`, `Name (string)`, `Description (string?)`, `Color (string, default "#003087")`, `IsDeleted (bool, default false)`, `CreatedDate (DateTime)`, `UpdatedDate (DateTime?)`, `UserId (int FK)`, navigation: `User User`, `ICollection<TaskItem> Tasks`.

**`TaskItem.cs`** — Define `TaskPriority` enum in the same namespace: `Low=0, Medium=1, High=2, Critical=3`. Entity properties: `Id (int PK)`, `Task (string)`, `Description (string?)`, `Priority (TaskPriority, default Medium)`, `DueDate (DateTime?)`, `IsCompleted (bool, default false)`, `IsDeleted (bool, default false)`, `CreatedDate (DateTime)`, `UpdatedDate (DateTime?)`, `CompletedDate (DateTime?)`, `UserId (int FK)`, navigation: `User User`, `ProjectId (int? FK)`, navigation: `Project? Project`.

---

### STEP 2: Application Layer (`src/Core/Application`)

Create `Application.csproj` targeting `net8.0`, referencing Domain project. Add NuGet: `BCrypt.Net-Next`.

**`ApiResponse<T>`** — Generic wrapper with: `bool Success`, `string Message`, `T? Data`, `List<string>? Errors`. Add static factory methods:
- `Ok(T data, string message = "Success")` → sets Success = true
- `Fail(string message, List<string>? errors = null)` → sets Success = false

**`IJwtHelper.cs`** — Interface with `string GenerateToken(User user)`.

**Auth DTOs:**
- `RegisterRequestDto`: `Username (Required, 2–100 chars)`, `Email (Required, EmailAddress)`, `Password (Required, MinLength 8, Regex for uppercase+number)`, `ConfirmPassword (Required, Compare "Password")`
- `LoginRequestDto`: `Email (Required, EmailAddress)`, `Password (Required)`
- `LoginResponseDto`: `Token (string)`, `Username (string)`, `Email (string)`, `UserId (int)`

**Todo DTOs:**
- `CreateTaskDto`: `Task (Required, 1–500 chars)`, `Description (optional, max 1000)`, `Priority (TaskPriority)`, `DueDate (DateTime?, must not be past)`, `ProjectId (int?)`
- `TaskResponseDto`: `Id`, `Task`, `Description?`, `Priority`, `PriorityLabel`, `DueDate?`, `IsOverdue`, `IsCompleted`, `CreatedDate`, `CompletedDate?`, `ProjectId?`, `ProjectName?`, `ProjectColor?`
- `UpdateTaskStatusDto`: `IsCompleted (bool)`
- `PaginatedResultDto<T>`: `Items (List<T>)`, `TotalCount`, `Page`, `PageSize`, `TotalPages`
- `DashboardDto`: `TotalTasks`, `CompletedTasks`, `RemainingTasks`, `CompletedToday`, `CompletedTodayPercentage`, `OverdueTasks`, `HighPriorityTasks`, `TotalProjects`, `ProjectSummaries (List<ProjectSummaryDto>)`
- `ProjectSummaryDto`: `ProjectId`, `ProjectName`, `ProjectColor`, `TaskCount`, `CompletedCount`

**Project DTOs:**
- `CreateProjectDto`: `Name (Required, 1–100 chars)`, `Description (optional, max 500)`, `Color (optional hex, default #003087)`
- `UpdateProjectDto`: `Name (Required, 1–100 chars)`, `Description (optional)`, `Color (optional)`
- `ProjectResponseDto`: `Id`, `Name`, `Description?`, `Color`, `TaskCount`, `CompletedCount`, `CreatedDate`

**Repository Interfaces:**
- `IUserRepository`: `GetByEmailAsync`, `CreateAsync`, `EmailExistsAsync`
- `ITaskRepository`: `GetPagedAsync(userId, page, pageSize, search?, projectId?)`, `GetTotalCountAsync(userId, search?, projectId?)`, `CreateAsync`, `GetByIdAsync(id, userId)`, `UpdateAsync`, `SoftDeleteAsync`, `GetDashboardDataAsync(userId)`
- `IProjectRepository`: `GetAllAsync(userId)`, `GetByIdAsync(id, userId)`, `CreateAsync`, `UpdateAsync`, `SoftDeleteAsync`, `GetTaskCountsAsync(userId)`

**Service Interfaces:**
- `IAuthService`: `RegisterAsync(dto, ct)`, `LoginAsync(dto, ct)`
- `ITaskService`: `GetTasksAsync(userId, page, pageSize, search?, projectId?, ct)`, `GetDashboardAsync(userId, ct)`, `CreateTaskAsync(userId, dto, ct)`, `UpdateTaskStatusAsync(userId, taskId, dto, ct)`, `DeleteTaskAsync(userId, taskId, ct)`
- `IProjectService`: `GetProjectsAsync(userId, ct)`, `CreateProjectAsync(userId, dto, ct)`, `UpdateProjectAsync(userId, id, dto, ct)`, `DeleteProjectAsync(userId, id, ct)`

**Service Implementations:**

`AuthService.cs`:
- `RegisterAsync`: validate email uniqueness, hash password with BCrypt workFactor 12, save user, return `ApiResponse.Ok`
- `LoginAsync`: find user by email, verify BCrypt hash, call `JwtHelper.GenerateToken`, return `LoginResponseDto`

`TaskService.cs`:
- All methods filter by `UserId` and `IsDeleted == false`
- `GetTasksAsync`: returns `PaginatedResultDto<TaskResponseDto>`; map `Priority` to `PriorityLabel` string; set `IsOverdue = DueDate < UtcNow && !IsCompleted`
- `CreateTaskAsync`: create `TaskItem`, set `CreatedDate = DateTime.UtcNow`
- `UpdateTaskStatusAsync`: set `CompletedDate = UtcNow` when completing, null when unchecking; set `UpdatedDate`
- `DeleteTaskAsync`: set `IsDeleted = true`, set `UpdatedDate = UtcNow`
- `GetDashboardAsync`: aggregate counts for total/completed/remaining/overdue/highPriority; calculate `CompletedTodayPercentage`; include per-project summaries

`ProjectService.cs`:
- `GetProjectsAsync`: return all non-deleted projects for user with task counts
- `CreateProjectAsync`: create `Project`, set `CreatedDate = DateTime.UtcNow`
- `UpdateProjectAsync`: find by id+userId, update fields, set `UpdatedDate`
- `DeleteProjectAsync`: set `IsDeleted = true`, set `UpdatedDate = UtcNow`

---

### STEP 3: Persistence Layer (`src/Infrastructure/Persistence`)

Create `Persistence.csproj` targeting `net8.0`, referencing Domain + Application. Add NuGet: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`.

**`AppDbContext.cs`**:
```csharp
// DbSet<User> Users
// DbSet<Project> Projects
// DbSet<TaskItem> Tasks
// OnModelCreating:
//   - TBL_User: unique index on Email; nvarchar column sizes
//   - TBL_Project: index on UserId; nvarchar column sizes; FK to TBL_User NO ACTION
//   - TBL_Task: index on (UserId, IsDeleted); FK to TBL_User CASCADE; FK to TBL_Project SET NULL
//   - Priority stored as int
```

**`UserRepository.cs`** — Implements `IUserRepository`. All methods async with `CancellationToken`.

**`TaskRepository.cs`** — Implements `ITaskRepository`.
- `GetPagedAsync`: filter by `userId`, `!IsDeleted`, optional `Contains(search)`, optional `projectId`, order by `CreatedDate DESC`, skip/take; include `Project` nav property
- `GetTotalCountAsync`: same filters, return `CountAsync()`
- `GetDashboardDataAsync`: return aggregated counts (overdue = DueDate < UtcNow && !IsCompleted, completedToday = CompletedDate date == today UTC)

**`ProjectRepository.cs`** — Implements `IProjectRepository`.
- `GetAllAsync`: filter by `userId`, `!IsDeleted`, order by `CreatedDate DESC`; include task counts
- `GetTaskCountsAsync`: return dictionary of projectId → (total, completed) counts

---

### STEP 4: Infrastructure Layer (`src/Infrastructure/Infrastructure`)

Create `Infrastructure.csproj` referencing Domain + Application. Add NuGet: `System.IdentityModel.Tokens.Jwt`.

**`JwtHelper.cs`** — Implements `IJwtHelper`:
```csharp
// Constructor: IConfiguration config
// GenerateToken(User user) -> string
// Claims: NameIdentifier (userId.ToString()), Email, Name (username)
// Signing: HmacSha256 with config["JwtSettings:SecretKey"]
// Expiry: DateTime.UtcNow.AddMinutes(config["JwtSettings:ExpiryInMinutes"])
// Issuer/Audience: from config["JwtSettings:Issuer/Audience"]
```

---

### STEP 5: Todo.API Layer (`src/Presentation/Todo.API`)

Create `Todo.API.csproj` targeting `net8.0`, referencing Application + Persistence + Infrastructure. Add NuGet: `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore`.

**`ExceptionMiddleware.cs`** — Catches all unhandled exceptions, logs them, returns `ApiResponse<object>` with `success: false`. Never expose stack traces in production.

**`AuthController.cs`** — Route `/api/auth`:
- `POST /register` → 201 Created on success
- `POST /login` → 200 OK with `LoginResponseDto`

**`TaskController.cs`** — Route `/api/tasks`, `[Authorize]`:
- `GET /` — query params: `page (default 1)`, `pageSize (default 10)`, `search?`, `projectId?`
- `POST /` → 201 Created
- `PATCH /{id}/status` → 200 OK
- `DELETE /{id}` → 200 OK

All actions extract userId: `int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)`

**`Program.cs`** (Todo.API):
```csharp
// Services:
// - AddDbContext with UseSqlServer, EnableRetryOnFailure
// - AddAuthentication(JwtBearer) with ValidateIssuer/Audience/Lifetime/SigningKey
// - AddScoped for all repositories and services
// - AddCors for http://localhost:4200 (or Todo.Web origin)
// - AddSwaggerGen with JWT Bearer auth scheme
// - AddControllers

// Middleware pipeline:
// - UseExceptionMiddleware (custom)
// - UseSwagger/UseSwaggerUI (dev only)
// - UseCors
// - UseAuthentication + UseAuthorization
// - MapControllers
```

---

### STEP 6: Todo.Web Layer (`src/Presentation/Todo.Web`)

Create `Todo.Web.csproj` targeting `net8.0` using `Microsoft.NET.Sdk.Web`, referencing Application + Persistence + Infrastructure (NOT Todo.API).

**`Program.cs`** (Todo.Web):
```csharp
// Services:
// - AddDbContext with UseSqlServer, EnableRetryOnFailure
// - AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(LoginPath=/Auth/Login, LogoutPath=/Auth/Logout,
//                ExpireTimeSpan=24h, SlidingExpiration=true,
//                Cookie.HttpOnly=true, Cookie.SecurePolicy=SameAsRequest)
// - AddScoped for all repositories and services
// - AddRazorPages
// - AddHttpContextAccessor

// Middleware pipeline:
// - UseExceptionHandler("/Error") + UseHsts (non-dev)
// - UseHttpsRedirection
// - UseStaticFiles
// - UseRouting
// - UseAuthentication + UseAuthorization
// - MapRazorPages
```

**`_ViewImports.cshtml`**:
```cshtml
@using Todo.Web
@using Todo.Web.Pages
@namespace Todo.Web.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

**`_ViewStart.cshtml`**:
```cshtml
@{ Layout = "_Layout"; }
```

**`_Layout.cshtml`** — Full HTML shell:
- `<html lang="en" data-bs-theme="light" id="htmlRoot">`
- Bootstrap 5.3 CSS + Bootstrap Icons CSS from CDN in `<head>`
- Navbar (shown only when `User.Identity?.IsAuthenticated == true`):
  - `bg-primary` with `data-bs-theme="dark"`
  - Brand: `<i class="bi bi-check2-square">` + "ICA Todo" linking to Dashboard
  - Nav links: Dashboard, Tasks, Projects — highlight active via `ViewData["ActivePage"]`
  - Right: theme toggle button (`id="themeToggle"`) + user dropdown with avatar circle + Sign Out form
- `@RenderBody()` inside `.container-fluid.py-4.px-4` when authenticated
- Bootstrap JS bundle from CDN
- `<script src="~/js/site.js">`
- `@await RenderSectionAsync("Scripts", required: false)`

**`Pages/Auth/Login.cshtml.cs`** (`LoginModel : PageModel`):
- `[BindProperty] LoginRequestDto Input`
- `OnGet`: redirect to Dashboard if already authenticated
- `OnPostAsync`: validate ModelState, call `_authService.LoginAsync`, on success build `ClaimsPrincipal` with `NameIdentifier/Name/Email` claims and call `HttpContext.SignInAsync` (cookie, IsPersistent=true, 24h expiry), redirect to Dashboard; on failure set `ErrorMessage` and return `Page()`

**`Pages/Auth/Register.cshtml.cs`** (`RegisterModel : PageModel`):
- `[BindProperty] RegisterRequestDto Input`
- `OnGet`: redirect to Dashboard if already authenticated
- `OnPostAsync`: validate ModelState, call `_authService.RegisterAsync`, on success redirect to Login with success message; on failure set `ErrorMessage` and return `Page()`

**`Pages/Auth/Logout.cshtml.cs`** (`LogoutModel : PageModel`):
- `[Authorize]`
- `OnPostAsync`: call `HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)`, redirect to Login

**`Pages/Index.cshtml.cs`**:
- `OnGet`: `return RedirectToPage("/Dashboard/Index")`

**`Pages/Dashboard/Index.cshtml.cs`** (`IndexModel : PageModel`):
- `[Authorize]`
- `public DashboardDto Dashboard { get; set; }`
- `OnGetAsync`: call `_taskService.GetDashboardAsync(GetUserId(), ct)`, set `ViewData["ActivePage"] = "Dashboard"`

**`Pages/Tasks/Index.cshtml.cs`** (`IndexModel : PageModel`):
- `[Authorize]`
- `[BindProperty(SupportsGet=true)] int Page`, `string? Search`, `int? ProjectFilter`
- `[BindProperty] CreateTaskDto NewTask`
- `public PaginatedResultDto<TaskResponseDto> Tasks`
- `public List<ProjectResponseDto> Projects`
- `OnGetAsync`: load tasks + projects
- `OnPostCreateAsync`: validate, create task
- `OnPostToggleAsync(int id, bool isCompleted)`: toggle, redirect
- `OnPostDeleteAsync(int id)`: delete, redirect
- `GetUserId()`: `int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)`

**`Pages/Projects/Index.cshtml.cs`** (`IndexModel : PageModel`):
- `[Authorize]`
- `[BindProperty] CreateProjectDto NewProject`
- `[BindProperty] UpdateProjectDto EditProject`
- `public List<ProjectResponseDto> Projects`
- `OnGetAsync`, `OnPostCreateAsync`, `OnPostEditAsync(int id)`, `OnPostDeleteAsync(int id)`
- Remove unrelated ModelState keys before validating (e.g., `ModelState.Remove("EditProject.Name")` in Create handler)

**`Pages/Tasks/Index.cshtml`** — Key layout elements:
- Task creation card with `asp-page-handler="Create"` form
- Search input + project filter dropdown (GET form with `method="get"`)
- Task list: each card shows priority badge, task title, description, due date, overdue warning
- Checkbox form with `asp-page-handler="Toggle"`, hidden inputs for `id` and `isCompleted`
- Delete button form with `asp-page-handler="Delete"`, hidden input for `id`
- Pagination: previous/next + page number links using `asp-page` + `asp-route-*`
- Completed tasks: `text-decoration: line-through; color: #dc3545`
- Priority badge colors: Low=`bg-success`, Medium=`bg-warning text-dark`, High=`bg-danger`, Critical=`bg-dark`

**`Pages/Dashboard/Index.cshtml`** — Stat cards:
- Total Tasks, Completed, Remaining, Overdue (each in a Bootstrap card with icon)
- Today's completion percentage (`progress` bar)
- High Priority tasks count
- Projects list with per-project task counts (`progress` bars)

**`Pages/Projects/Index.cshtml`** — Project management:
- Create project form (name, description, color picker)
- Project cards with edit (inline modal or inline form) and delete buttons
- Task count badges per project

**`wwwroot/js/site.js`** — Theme toggle:
```javascript
// On DOMContentLoaded:
//   1. Read ica-theme from localStorage, apply via htmlRoot.setAttribute('data-bs-theme', ...)
//   2. themeToggle click: toggle between 'light' and 'dark', save to localStorage
//   3. Update themeIcon class: 'bi bi-sun-fill' for dark mode, 'bi bi-moon-stars-fill' for light
```

**`wwwroot/css/site.css`** — Custom styles:
```css
/* ICA brand colors */
:root { --ica-primary: #003087; }

/* Avatar circle in navbar */
.avatar-circle { ... }

/* Completed task strikethrough */
.task-completed { text-decoration: line-through; color: #dc3545; }

/* Overdue indicator */
.task-overdue { border-left: 3px solid #dc3545; }
```

---

## appsettings.json (both projects — do NOT commit real credentials)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=IcaTodoDB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;Encrypt=False;"
  },
  "JwtSettings": {
    "SecretKey": "IcaTodoAppSuperSecretKey2024!MustBe32CharsMin",
    "Issuer": "IcaTodoApp",
    "Audience": "IcaTodoUsers",
    "ExpiryInMinutes": 1440
  },
  "AllowedHosts": "*",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## EF Core Migrations

```bash
# Run from solution root — Todo.Web is the startup project
dotnet ef migrations add InitialCreate \
  --project src/Infrastructure/Persistence \
  --startup-project src/Presentation/Todo.Web \
  --output-dir Migrations

dotnet ef database update \
  --project src/Infrastructure/Persistence \
  --startup-project src/Presentation/Todo.Web
```

---

## Final Checklist

Before considering the implementation complete, verify:

### Shared Layers
- [ ] All 4 `.csproj` files (Domain, Application, Persistence, Infrastructure) reference correct dependencies
- [ ] `IcaTodo.sln` includes all 6 projects
- [ ] `AppDbContext` uses `TBL_User`, `TBL_Task`, `TBL_Project` table names
- [ ] All repository methods are async with `CancellationToken`
- [ ] JWT claims include `NameIdentifier` (userId), `Email`, `Name` (username)
- [ ] BCrypt work factor 12
- [ ] Soft delete on all task and project deletions

### Todo.API
- [ ] All controller actions return `ApiResponse<T>`
- [ ] `[Authorize]` on `TaskController`
- [ ] UserId from JWT claims in every task action
- [ ] CORS configured
- [ ] Swagger configured with JWT Bearer auth scheme

### Todo.Web
- [ ] Cookie authentication configured (HttpOnly, 24h, sliding)
- [ ] All pages except Login/Register decorated with `[Authorize]`
- [ ] UserId extracted from cookie claims in every PageModel
- [ ] POST handlers use `RedirectToPage` after success (PRG pattern)
- [ ] `ModelState.IsValid` checked in all POST handlers
- [ ] `asp-page`, `asp-page-handler`, `asp-for`, `asp-route-*` Tag Helpers used everywhere
- [ ] Dark/Light theme toggle persists to `localStorage('ica-theme')`
- [ ] `ViewData["ActivePage"]` set in every PageModel for navbar highlighting
- [ ] Dashboard shows all 7 stat metrics + project summaries
- [ ] Tasks page supports search, project filter, and pagination (10 per page)
- [ ] Task cards show priority badge, due date, overdue indicator
- [ ] Completed tasks show strikethrough red text
- [ ] Projects page supports create, edit, delete

### Database
- [ ] SQL Server connection string configured
- [ ] EF Core migrations created and applied
- [ ] Unique index on `TBL_User.Email`
- [ ] Composite index on `TBL_Task (UserId, IsDeleted)`
- [ ] FK from `TBL_Task.UserId` to `TBL_User.Id` with CASCADE DELETE
- [ ] FK from `TBL_Task.ProjectId` to `TBL_Project.Id` with SET NULL

---

## Running the Application

### Todo.Web (Razor Pages)
```bash
cd src/Presentation/Todo.Web
dotnet run
# App: https://localhost:7002
```

### Todo.API (REST API)
```bash
cd src/Presentation/Todo.API
dotnet run
# Swagger UI: https://localhost:7001/swagger
```
