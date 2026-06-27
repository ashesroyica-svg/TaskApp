using Application.DTOs.Auth;
using Application.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Todo.Web.Pages.Auth;

/// <summary>Handles new user registration.</summary>
public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;

    public RegisterModel(IAuthService authService) => _authService = authService;

    [BindProperty]
    public RegisterRequestDto Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Dashboard/Index");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _authService.RegisterAsync(Input, ct);

        if (!result.Success)
        {
            ErrorMessage = result.Message;
            return Page();
        }

        return RedirectToPage("/Auth/Login", new { message = "Registration successful! Please sign in." });
    }
}
