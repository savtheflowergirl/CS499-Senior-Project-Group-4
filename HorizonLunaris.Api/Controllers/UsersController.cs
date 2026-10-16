using HorizonLunaris.Api.Services;
using HorizonLunaris.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HorizonLunaris.Api.Controllers;

/// <summary>Lists the seeded demo users so the UI can offer an "acting as" picker. Fully implemented.</summary>
[ApiController]
[Route("api/users")]
public class UsersController(HorizonLunarisDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var users = await db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync(ct);
        return Ok(users.Select(u => u.ToDto()).ToList());
    }
}
