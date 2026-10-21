// Replaces App.xaml / App.xaml.cs: this is the application entry point and startup configuration.
var builder = WebApplication.CreateBuilder(args);

// Register MVC (controllers + Razor views)
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Equivalent of StartupUri="MainWindow.xaml": the Dashboard is the landing page.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
