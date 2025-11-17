using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sensore.Data;
using Sensore.Infrastructure.Auth;
using Sensore.Models;

var builder = WebApplication.CreateBuilder(args);


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


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();


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

await Sensore.Infrastructure.Seed.IdentitySeeder.SeedAsync(app.Services);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!db.Alerts.Any())
    {
        var anyUserId = db.Users.Select(u => u.Id).FirstOrDefault();
        if (anyUserId != null)
        {
            db.Alerts.Add(new Alert { UserId = anyUserId, Severity = "Critical", Reason = "Test seed" });
            db.SaveChanges();
        }
    }
}


app.Run();
