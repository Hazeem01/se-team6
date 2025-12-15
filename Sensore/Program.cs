using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sensore.Data;
using Sensore.Infrastructure.Auth;
using Sensore.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

/// <summary>
/// Configure Identity:
/// - Uses IdentityUser for user accounts
/// - Adds role support via IdentityRole
/// - Persists Identity data into ApplicationDbContext
/// Notes: Authentication UI is provided by the Identity area (Razor Pages). Controllers use
/// UserManager/RoleManager when managing accounts (AdminController).
/// </summary>
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization(options =>
{
    // Policies map to role constants in SensoreRoles
    options.AddPolicy("IsAdmin", p => p.RequireRole(SensoreRoles.Admin));
    options.AddPolicy("IsClinician", p => p.RequireRole(SensoreRoles.Clinician));
    options.AddPolicy("IsPatient", p => p.RequireRole(SensoreRoles.Patient));
    options.AddPolicy("IsDoctor", p => p.RequireRole(SensoreRoles.Doctor));
    options.AddPolicy("IsManager", p => p.RequireRole(SensoreRoles.Manager));
});

// Configure cookie paths for Identity area (Razor Pages)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying pending migrations (if any)...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed during startup.");
        throw;
    }

    // Seed sample sensor metrics if none exist - used for demo charts in the admin/manager UI.
    if (!db.SensorMetrics.Any())
    {
        var now = DateTime.UtcNow;
        var rand = new Random();

        var metrics = Enumerable.Range(0, 72).Select(i => new SensorMetric
        {
            Timestamp = now.AddMinutes(-5 * i),
            PeakPressureIndex = rand.Next(160, 240),
            ContactAreaPercentage = 55 + rand.NextDouble() * 20,
            AveragePressure = rand.Next(80, 140),
            HighPressureRegions = rand.Next(0, 4)
        });

        db.SensorMetrics.AddRange(metrics);
        db.SaveChanges();
    }
}

try
{
    // Seed roles and sample users for admin testing (development only).
    await Sensore.Infrastructure.Seed.IdentitySeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Failed to seed identity data.");
    throw;
}

app.Run();
