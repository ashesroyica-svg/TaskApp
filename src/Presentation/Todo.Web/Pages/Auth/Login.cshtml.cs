using System.Security.Claims;
using Application.DTOs.Auth;
using Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Todo.Web.Pages.Auth;

/// <summary>Handles user login and cookie sign-in.</summary>
public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService) => _authService = authService;

    [BindProperty]
    public LoginRequestDto Input { get; set; } = new();

    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }

    public IActionResult OnGet(string? message = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Dashboard/Index");

        Message = message;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _authService.LoginAsync(Input, ct);

        if (!result.Success)
        {
            ErrorMessage = result.Message;
            return Page();
        }

        var data = result.Data!;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, data.UserId.ToString()),
            new(ClaimTypes.Name, data.Username),
            new(ClaimTypes.Email, data.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24) });

        return RedirectToPage("/Dashboard/Index");
    }
}
