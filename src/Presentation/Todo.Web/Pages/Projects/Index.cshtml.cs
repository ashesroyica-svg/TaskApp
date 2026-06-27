using System.Security.Claims;
using Application.DTOs.Project;
using Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Todo.Web.Pages.Projects;

/// <summary>Project management page — create, edit, and delete projects.</summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IProjectService _projectService;

    public IndexModel(IProjectService projectService) => _projectService = projectService;

    public List<ProjectResponseDto> Projects { get; set; } = new();

    [BindProperty]
    public CreateProjectDto NewProject { get; set; } = new();

    [BindProperty]
    public UpdateProjectDto EditProject { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadProjectsAsync(ct);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
    {
        ModelState.Remove(nameof(EditProject) + ".Name");

        if (!ModelState.IsValid)
        {
            await LoadProjectsAsync(ct);
            return Page();
        }

        var result = await _projectService.CreateProjectAsync(GetUserId(), NewProject, ct);

        if (!result.Success)
            ErrorMessage = result.Message;
        else
            SuccessMessage = $"Project '{result.Data?.Name}' created successfully.";

        await LoadProjectsAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, CancellationToken ct)
    {
        ModelState.Remove(nameof(NewProject) + ".Task");
        ModelState.Remove(nameof(NewProject) + ".Name");

        if (!ModelState.IsValid)
        {
            await LoadProjectsAsync(ct);
            return Page();
        }

        var result = await _projectService.UpdateProjectAsync(GetUserId(), id, EditProject, ct);

        if (!result.Success)
            ErrorMessage = result.Message;
        else
            SuccessMessage = $"Project '{result.Data?.Name}' updated.";

        await LoadProjectsAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var result = await _projectService.DeleteProjectAsync(GetUserId(), id, ct);

        if (!result.Success)
            ErrorMessage = result.Message;
        else
            SuccessMessage = "Project deleted.";

        await LoadProjectsAsync(ct);
        return Page();
    }

    private async Task LoadProjectsAsync(CancellationToken ct = default)
    {
        var result = await _projectService.GetProjectsAsync(GetUserId(), ct);
        if (result.Success && result.Data != null)
            Projects = result.Data;

        ViewData["ActivePage"] = "Projects";
    }
}
