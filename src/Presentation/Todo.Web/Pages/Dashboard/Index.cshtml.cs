using System.Security.Claims;
using Application.DTOs.Todo;
using Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Todo.Web.Pages.Dashboard;

/// <summary>Dashboard page showing task and project statistics.</summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly ITaskService _taskService;

    public IndexModel(ITaskService taskService) => _taskService = taskService;

    public DashboardDto Dashboard { get; set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.GetDashboardAsync(userId, ct);

        if (result.Success && result.Data != null)
            Dashboard = result.Data;

        ViewData["ActivePage"] = "Dashboard";
    }
}
