using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Todo.Web.Pages;

/// <summary>Root page — redirects authenticated users to Dashboard, others to Login.</summary>
public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Dashboard/Index");

        return RedirectToPage("/Auth/Login");
    }
}
