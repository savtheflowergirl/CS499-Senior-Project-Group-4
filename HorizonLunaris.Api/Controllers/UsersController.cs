using HorizonLunaris.Api.Services;
using HorizonLunaris.Contracts;
using HorizonLunaris.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorizonLunaris.Api.Controllers;

/// <summary>Lists users for the prototype identity picker and provides Admin-only user management.</summary>
[ApiController]
[Route("api/users")]
public class UsersController(HorizonLunarisDbContext db, IUserManagementService userManagement) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync(ct);
        return Ok(users.Select(u => u.ToDto()).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken ct) =>
        Ok(await userManagement.CreateAsync(request, ct));

    [HttpPut("{userId:int}/email")]
    public async Task<IActionResult> UpdateEmail(int userId, UpdateUserEmailRequest request, CancellationToken ct) =>
        Ok(await userManagement.UpdateEmailAsync(userId, request, ct));
}
