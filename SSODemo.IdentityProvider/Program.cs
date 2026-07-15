using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using SSODemo.IdentityProvider;
using SSODemo.IdentityProvider.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("OpenIddict");
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetTokenEndpointUris("connect/token")
            .SetEndSessionEndpointUris("connect/endsession");

        options.AllowAuthorizationCodeFlow();

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.RegisterScopes(
            OpenIddictConstants.Scopes.OpenId,
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Email);

        options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .DisableTransportSecurityRequirement();
    });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.Name = ".IdP.Auth";
    });

builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<SeedData>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Content("""
<!DOCTYPE html>
<html>
<head>
    <title>SSO Identity Provider</title>
    <style>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; max-width: 600px; margin: 60px auto; padding: 0 20px; color: #333; }
        .badge { background: #6f42c1; color: white; padding: 16px 24px; border-radius: 10px; margin-bottom: 24px; }
        .badge h1 { margin: 0 0 4px; font-size: 22px; }
        .badge p { margin: 0; opacity: 0.85; font-size: 14px; }
        .card { background: #f8f9fa; border-radius: 10px; padding: 20px; margin-bottom: 16px; }
        .card h3 { margin: 0 0 8px; }
        a { color: #6f42c1; }
    </style>
</head>
<body>
    <div class="badge">
        <h1>Identity Provider</h1>
        <p>SSO Demo - Central Authentication Server (port 5001)</p>
    </div>
    <div class="card">
        <h3>Connected Applications</h3>
        <p><a href="http://localhost:5011">App One (Blue)</a> - port 5011</p>
        <p><a href="http://localhost:5021">App Two (Green)</a> - port 5021</p>
    </div>
    <div class="card">
        <h3>Demo Accounts</h3>
        <p><strong>alice</strong> / password</p>
        <p><strong>bob</strong> / password</p>
    </div>
</body>
</html>
""", "text/html"));

app.Run();
