# CLAUDE.md — Claude Code Instructions for ICA Todo Application

## Project Context

This is a full-stack ICA Employee Todo Application built with:
- **Backend API:** ASP.NET Core 8 Web API (Clean Architecture) — `Todo.API`
- **Web Frontend:** ASP.NET Core 8 Razor Pages — `Todo.Web`
- **Database:** SQL Server via EF Core (Code-First, `Microsoft.EntityFrameworkCore.SqlServer`)
- **Auth in Todo.Web:** Cookie authentication (`Microsoft.AspNetCore.Authentication.Cookies`)
- **Auth in Todo.API:** JWT Bearer tokens

Read `SPEC.md` and `project-spec.md` for full requirements before generating any code.

---

## Code Generation Rules

### General

- Always generate **complete, runnable files** — no placeholders, no `// TODO` comments
- Follow the exact folder structure defined in `SPEC.md`
- Every file must compile/run without modification
- Use **UTF-8 encoding** for all files
- Add XML doc comments on all public C# methods and classes

### C# / .NET Conventions

- Target framework: `net8.0`
- Nullable reference types: **enabled** (`<Nullable>enable</Nullable>`)
- Implicit usings: **enabled**
- Use `var` for local variables where type is obvious
- Use `async/await` for all I/O operations
- Controller actions must return `Task<IActionResult>`
- PageModel handler methods must return `Task<IActionResult>` or `Task` (for OnGet)
- Use `ApiResponse<T>` wrapper for **all** API responses (Todo.API)
- Use `[Required]`, `[EmailAddress]`, `[MinLength]` data annotations on all DTOs
- Use `ILogger<T>` in all services and controllers
- Repository methods must be async and cancellable (`CancellationToken ct = default`)
- `AppDbContext` table names: `TBL_User`, `TBL_Task`, `TBL_Project`
- Use `HasColumnType("nvarchar(255)")` etc. in EF fluent configuration

**Naming Conventions:**
- Interfaces: `IServiceName`, `IRepositoryName`
- DTOs: `EntityActionDto` (e.g., `RegisterRequestDto`, `TaskResponseDto`)
- Services: `EntityService` (e.g., `AuthService`)
- Repositories: `EntityRepository`
- Controllers: `EntityController` (Todo.API)
- Razor Pages: `Pages/{Feature}/Index.cshtml` + `Index.cshtml.cs` with `IndexModel : PageModel`

### Razor Pages Conventions (Todo.Web)

- Use `[BindProperty]` for form-bound properties on PageModel
- Use `[BindProperty(SupportsGet = true)]` for query-string bound properties (pagination, search, filters)
- Handler methods: `OnGetAsync`, `OnPostAsync`, `OnPostCreateAsync`, `OnPostEditAsync`, `OnPostDeleteAsync`, `OnPostToggleAsync`
- Use `asp-page`, `asp-page-handler`, `asp-route-*` Tag Helpers — never raw `action=` attributes
- Use `asp-for` on form fields — never raw `name=` attributes
- Extract UserId from cookie claims: `int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)`
- Redirect after POST to avoid double-submit: `return RedirectToPage(new { Page, Search })`
- Use `ViewData["ActivePage"]` to highlight the current nav item in `_Layout.cshtml`
- Use `[Authorize]` attribute on every PageModel that requires login
- Unauthenticated users are automatically redirected to `/Auth/Login` by cookie middleware

### CSS / Styling Conventions

- Use Bootstrap 5.3 utility classes as the primary styling approach
- Custom CSS lives in `wwwroot/css/site.css`
- Dark mode: toggle `data-bs-theme` attribute on `<html id="htmlRoot">` via JavaScript; persist to `localStorage('ica-theme')`
- ICA brand color: `#003087` (navy blue) as primary accent
- Completed task style: `text-decoration: line-through; color: #dc3545;`
- Navbar: always use `bg-primary` with `data-bs-theme="dark"` navbar variant
- Cards: `shadow-sm` on all task/project cards
- Priority badges: Low = `bg-success`, Medium = `bg-warning text-dark`, High = `bg-danger`, Critical = `bg-dark`

---

## File Generation Order

Generate files in this order to avoid dependency issues:

### Backend Order (shared layers)
1. `Domain/Entities/User.cs`
2. `Domain/Entities/Project.cs`
3. `Domain/Entities/TaskItem.cs` (with `TaskPriority` enum)
4. `Application/Wrappers/ApiResponse.cs`
5. `Application/DTOs/Auth/*.cs`
6. `Application/DTOs/Todo/*.cs`
7. `Application/DTOs/Project/*.cs`
8. `Application/RepositoryInterfaces/IUserRepository.cs`
9. `Application/RepositoryInterfaces/ITaskRepository.cs`
10. `Application/RepositoryInterfaces/IProjectRepository.cs`
11. `Application/ServiceInterfaces/IAuthService.cs`
12. `Application/ServiceInterfaces/ITaskService.cs`
13. `Application/ServiceInterfaces/IProjectService.cs`
14. `Application/ServiceInterfaces/IJwtHelper.cs`
15. `Persistence/Context/AppDbContext.cs`
16. `Persistence/Repositories/UserRepository.cs`
17. `Persistence/Repositories/TaskRepository.cs`
18. `Persistence/Repositories/ProjectRepository.cs`
19. `Infrastructure/Helpers/JwtHelper.cs`
20. `Application/Services/AuthService.cs`
21. `Application/Services/TaskService.cs`
22. `Application/Services/ProjectService.cs`

### Todo.API Order
23. `Todo.API/Controllers/AuthController.cs`
24. `Todo.API/Controllers/TaskController.cs`
25. `Todo.API/Middleware/ExceptionMiddleware.cs`
26. `Todo.API/Program.cs`
27. `Todo.API/appsettings.json`

### Todo.Web Order (Razor Pages)
28. `Todo.Web/Pages/Shared/_ViewImports.cshtml`
29. `Todo.Web/Pages/Shared/_ViewStart.cshtml`
30. `Todo.Web/Pages/Shared/_Layout.cshtml`
31. `Todo.Web/Pages/Auth/Login.cshtml` + `Login.cshtml.cs`
32. `Todo.Web/Pages/Auth/Register.cshtml` + `Register.cshtml.cs`
33. `Todo.Web/Pages/Auth/Logout.cshtml` + `Logout.cshtml.cs`
34. `Todo.Web/Pages/Index.cshtml` + `Index.cshtml.cs`
35. `Todo.Web/Pages/Dashboard/Index.cshtml` + `Index.cshtml.cs`
36. `Todo.Web/Pages/Tasks/Index.cshtml` + `Index.cshtml.cs`
37. `Todo.Web/Pages/Projects/Index.cshtml` + `Index.cshtml.cs`
38. `Todo.Web/Pages/Error.cshtml` + `Error.cshtml.cs`
39. `Todo.Web/wwwroot/css/site.css`
40. `Todo.Web/wwwroot/js/site.js`
41. `Todo.Web/Program.cs`
42. `Todo.Web/appsettings.json`

### Solution Files
43. `.csproj` files for each project
44. `IcaTodo.sln`

---

## Key Implementation Details

### UserId Extraction (Razor Pages)

Always extract UserId from cookie claims in the PageModel — never from form fields:

```csharp
private int GetUserId() =>
    int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
```

### UserId Extraction (Todo.API Controllers)

```csharp
private int GetUserIdFromToken()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null) throw new UnauthorizedAccessException();
    return int.Parse(userIdClaim.Value);
}
```

### Soft Delete Pattern

Never hard-delete tasks. Always set `IsDeleted = true` and filter in all queries:

```csharp
// Repository — always filter deleted tasks
.Where(t => t.UserId == userId && !t.IsDeleted)
```

### Cookie Authentication Setup (Todo.Web Program.cs)

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });
```

### Sign-In After Login (Razor Pages)

```csharp
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, data.UserId.ToString()),
    new(ClaimTypes.Name, data.Username),
    new(ClaimTypes.Email, data.Email)
};
var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(identity),
    new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24) });
```

### BCrypt Usage (Backend)

```csharp
// Hash on register
string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

// Verify on login
bool valid = BCrypt.Net.BCrypt.Verify(password, storedHash);
```

### EF Core Configuration (SQL Server)

```csharp
// AppDbContext — table naming
modelBuilder.Entity<User>().ToTable("TBL_User");
modelBuilder.Entity<TaskItem>().ToTable("TBL_Task");
modelBuilder.Entity<Project>().ToTable("TBL_Project");

// Indexes for performance
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email).IsUnique();
modelBuilder.Entity<TaskItem>()
    .HasIndex(t => new { t.UserId, t.IsDeleted });
modelBuilder.Entity<Project>()
    .HasIndex(p => p.UserId);
```

### Dark Mode Toggle (site.js)

```javascript
// Toggle theme and persist to localStorage
const htmlRoot = document.getElementById('htmlRoot');
const themeToggle = document.getElementById('themeToggle');
const themeIcon = document.getElementById('themeIcon');

function applyTheme(theme) {
    htmlRoot.setAttribute('data-bs-theme', theme);
    themeIcon.className = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill';
}
// On load: read from localStorage('ica-theme')
// On click: toggle and save
```

---

## Error Handling

### Backend (Todo.API)

- Global exception middleware catches all unhandled exceptions
- Returns `ApiResponse<object>` with `success: false` and error message
- Never expose stack traces in production
- Log all exceptions with `ILogger`

### Frontend (Todo.Web Razor Pages)

- Service layer returns `ApiResponse<T>` — check `.Success` in PageModel
- On failure, set `ErrorMessage` property and call `return Page()` to re-render
- On success after POST, set `SuccessMessage` and reload data, then `return Page()`
- `[Authorize]` attribute handles unauthenticated access (redirects to `/Auth/Login`)
- ModelState validation errors are shown via `asp-validation-for` Tag Helpers

---

## EF Core Migrations

```bash
# Run from solution root
dotnet ef migrations add InitialCreate \
  --project src/Infrastructure/Persistence \
  --startup-project src/Presentation/Todo.Web

dotnet ef database update \
  --project src/Infrastructure/Persistence \
  --startup-project src/Presentation/Todo.Web
```

---

## Do Not

- Do NOT use `ViewBag` — use `ViewData` (for layout helpers like `ActivePage`) or strongly-typed Model properties
- Do NOT use MVC `View()` returns in Razor Pages — use `Page()`, `RedirectToPage()`, `RedirectToAction()`
- Do NOT store passwords in plain text — always BCrypt hash
- Do NOT trust UserId from form fields — always use cookie claims (`ClaimTypes.NameIdentifier`)
- Do NOT hard-delete any task records — use soft delete (`IsDeleted = true`)
- Do NOT skip form validation on backend — always check `ModelState.IsValid`
- Do NOT add `console.log` debug statements in production code
- Do NOT use raw `<form action="...">` — always use Tag Helpers (`asp-page`, `asp-page-handler`)
- Do NOT add Angular, React, or any SPA framework — this is a server-rendered Razor Pages app
- Do NOT use Node.js / npm / Angular CLI — no JavaScript build pipeline
