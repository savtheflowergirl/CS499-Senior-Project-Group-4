using System.Text.Json.Serialization;
using HorizonLunaris.Api.Services;
using HorizonLunaris.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---- Core ----
builder.Services.AddDbContext<HorizonLunarisDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// ---- Services: ADD YOUR OWN LINE ONLY. Do not reorder or reformat other lines. ----
builder.Services.AddScoped<ICurrentUser, HeaderCurrentUser>();
builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();
builder.Services.AddScoped<IProjectTaskService, ProjectTaskService>();   // Feature 1
builder.Services.AddScoped<ITaskProgressService, TaskProgressService>(); // Feature 2
builder.Services.AddScoped<IDependencyService, DependencyService>();     // Feature 3

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Create/upgrade the local SQLite database on startup (requires the committed InitialCreate migration).
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<HorizonLunarisDbContext>().Database.Migrate();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Lets a test project reference the app if you later add integration tests.
public partial class Program;
