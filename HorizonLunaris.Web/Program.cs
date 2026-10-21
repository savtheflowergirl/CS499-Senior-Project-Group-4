using HorizonLunaris.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// Typed client for the API. Every call carries the "acting as" user via UserHeaderHandler.
builder.Services.AddTransient<UserHeaderHandler>();
builder.Services.AddHttpClient<IHorizonApiClient, HorizonApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<UserHeaderHandler>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSession();
app.UseStaticFiles();
app.MapRazorPages();

app.Run();
