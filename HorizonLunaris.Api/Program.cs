
using System.Text.Json.Serialization;
using HorizonLunaris.Api.Services;
using HorizonLunaris.Data;
using HorizonLunaris.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---- Core ----
builder.Services.AddDbContext<HorizonLunarisDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
})
.AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// ---- Services: ADD YOUR OWN LINE ONLY. Do not reorder or reformat other lines. ----
builder.Services.AddScoped<ICurrentUser, AuthenticatedCurrentUser>();
builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();
builder.Services.AddScoped<IProjectTaskService, ProjectTaskService>();   // Feature 1
builder.Services.AddScoped<ITaskProgressService, TaskProgressService>(); // Feature 2
builder.Services.AddScoped<IDependencyService, DependencyService>();     // Feature 3
builder.Services.AddScoped<ITaskReviewService, TaskReviewService>();     // Manual Review Gate;
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// ---- Shared Authentication; SM ----
var keyDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".horizonlunaris",
    "auth-keys");

Directory.CreateDirectory(keyDirectory);

builder.Services.AddDataProtection()
    .SetApplicationName("HorizonLunaris.SharedAuth")
    .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));

// ---- User Authentication; SM ----
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "HorizonLunaris.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

// Require authentication for API endpoints by default
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy =
        new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

// ---- CSRF Protection; SM ----
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "HorizonLunaris.CSRF";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    scope.ServiceProvider
        .GetRequiredService<HorizonLunarisDbContext>()
        .Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Allows integration tests to reference the application
public partial class Program;

