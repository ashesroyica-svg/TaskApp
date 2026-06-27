using System.Security.Claims;
using Application.DTOs.Project;
using Application.DTOs.Todo;
using Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Todo.Web.Pages.Tasks;

/// <summary>Task management page with create, toggle, delete, and pagination.</summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly ITaskService _taskService;
    private readonly IProjectService _projectService;

    public IndexModel(ITaskService taskService, IProjectService projectService)
    {
        _taskService = taskService;
        _projectService = projectService;
    }

    public PaginatedResultDto<TaskResponseDto> Tasks { get; set; } = new();
    public List<ProjectResponseDto> Projects { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public new int Page { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? ProjectFilter { get; set; }

    [BindProperty]
    public CreateTaskDto NewTask { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadDataAsync(ct);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync(ct);
            return Page();
        }

        var result = await _taskService.CreateTaskAsync(GetUserId(), NewTask, ct);

        if (!result.Success)
            ErrorMessage = result.Message;
        else
            SuccessMessage = "Task created successfully.";

        await LoadDataAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id, bool isCompleted, CancellationToken ct)
    {
        await _taskService.UpdateTaskStatusAsync(GetUserId(), id, new UpdateTaskStatusDto { IsCompleted = isCompleted }, ct);
        return RedirectToPage(new { Page, Search, ProjectFilter });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        await _taskService.DeleteTaskAsync(GetUserId(), id, ct);
        return RedirectToPage(new { Page, Search, ProjectFilter });
    }

    private async Task LoadDataAsync(CancellationToken ct = default)
    {
        var userId = GetUserId();
        var tasksResult = await _taskService.GetTasksAsync(userId, Page, 10, Search, ProjectFilter, ct);
        var projectsResult = await _projectService.GetProjectsAsync(userId, ct);

        if (tasksResult.Success && tasksResult.Data != null)
            Tasks = tasksResult.Data;

        if (projectsResult.Success && projectsResult.Data != null)
            Projects = projectsResult.Data;

        ViewData["ActivePage"] = "Tasks";
    }
}
