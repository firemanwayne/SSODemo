using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace SSODemo.IdentityProvider.Controllers;

public class AccountController : Controller
{
    private static readonly Dictionary<string, (string Password, string DisplayName)> Users = new()
    {
        ["alice"] = ("password", "Alice Smith"),
        ["bob"] = ("password", "Bob Jones"),
    };

    [HttpGet("~/Account/Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost("~/Account/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(username) &&
            Users.TryGetValue(username.ToLower(), out var user) &&
            user.Password == password)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.DisplayName),
                new(ClaimTypes.NameIdentifier, username.ToLower()),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("/");
        }

        ViewBag.Error = "Invalid username or password.";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }
}
