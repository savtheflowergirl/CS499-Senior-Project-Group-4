
using System.Security.Claims;
using HorizonLunaris.Contracts;
using HorizonLunaris.Data;
using HorizonLunaris.Domain.Entities;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorizonLunaris.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    HorizonLunarisDbContext db,
    IPasswordHasher<User> passwordHasher,
    IAntiforgery antiforgery) : ControllerBase
{
    // Log in using email and password
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email and password are required.");
        }

        var email = request.Email.Trim();

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        var result = passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Invalid email or password.");
        }

        // Create claims for the authenticated user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        // Create the authentication identity
        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var principal = new ClaimsPrincipal(identity);

        // Sign in and create the authentication cookie
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        return Ok(new LoginResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString()
        ));
    }

    // Log out the currently authenticated user
    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return Ok(new { message = "Logged out successfully." });
    }

    // Return information about the logged-in user
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Name = User.Identity?.Name,
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    // Issue a CSRF token for authenticated requests
    [Authorize]
    [HttpGet("csrf-token")]
    public IActionResult GetCsrfToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);

        Response.Headers.CacheControl = "no-store";

        return Ok(new
        {
            token = tokens.RequestToken
        });
    }
}

