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
    options.AddPolicy("IsAdmin", p => p.RequireRole(SensoreRoles.Admin));
    options.AddPolicy("IsClinician", p => p.RequireRole(SensoreRoles.Clinician));
    options.AddPolicy("IsPatient", p => p.RequireRole(SensoreRoles.Patient));
    options.AddPolicy("IsDoctor", p => p.RequireRole(SensoreRoles.Doctor));
    options.AddPolicy("IsManager", p => p.RequireRole(SensoreRoles.Manager));
});

//This is being used for better redirections
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

    // I am using this to seed sample sensor metrics if none exist
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
    await Sensore.Infrastructure.Seed.IdentitySeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Failed to seed identity data.");
    throw;
}

app.Run();
