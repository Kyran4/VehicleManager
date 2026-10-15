using Microsoft.EntityFrameworkCore;
using VehicleManager.Core.Services;
using VehicleManager.Infrastructure;
using VehicleManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddRazorPages();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<CategoryService>();

var app = builder.Build();

// Convenience for local development (enabled in appsettings.Development.json): apply EF migrations at startup.
// Reviewers can equally run `dotnet ef database update` - see README.
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

// Always use the friendly error page - stack traces are never shown to users (details go to the log).
app.UseExceptionHandler("/Error");
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
