
using HorizonLunaris.Data;
using HorizonLunaris.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var dbPath = Path.GetFullPath(
    "HorizonLunaris.Api/horizonlunaris.db");

if (!File.Exists(dbPath))
{
    Console.WriteLine($"Database not found: {dbPath}");
    return;
}

using var db = new HorizonLunarisDbContext(
    new DbContextOptionsBuilder<HorizonLunarisDbContext>()
        .UseSqlite($"Data Source={dbPath}")
        .Options);

var passwordHasher = new PasswordHasher<User>();

var accounts = new[]
{
    (Email: "pm@example.com", Env: "HL_PM_PASSWORD"),
    (Email: "member1@example.com", Env: "HL_MEMBER1_PASSWORD"),
    (Email: "member2@example.com", Env: "HL_MEMBER2_PASSWORD")
};

foreach (var account in accounts)
{
    var password = Environment.GetEnvironmentVariable(account.Env);

    if (string.IsNullOrWhiteSpace(password))
    {
        Console.WriteLine($"Skipping {account.Email}: {account.Env} is not set.");
        continue;
    }

    var user = await db.Users
        .FirstOrDefaultAsync(u => u.Email == account.Email);

    if (user is null)
    {
        Console.WriteLine($"User not found: {account.Email}");
        continue;
    }

    if (!string.IsNullOrEmpty(user.PasswordHash))
    {
        Console.WriteLine($"Password already exists for {account.Email}.");
        continue;
    }

    user.PasswordHash = passwordHasher.HashPassword(user, password);

    Console.WriteLine($"Password initialized for {account.Email}.");
}

await db.SaveChangesAsync();

Console.WriteLine("Password setup complete.");

