using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sensore.Data;
using Sensore.Infrastructure.Auth;
using Sensore.Models;

[Authorize(Policy = "IsManager")]        
public class ManagerController : Controller
{
    private readonly ApplicationDbContext _db;
    public ManagerController(ApplicationDbContext db) => _db = db;

    
    public IActionResult Index() => View();


    [HttpGet]
    public async Task<IActionResult> Alerts(DateTime? from = null, DateTime? to = null)
    {
        var q = _db.Alerts
                   .Include(a => a.User)       
                   .OrderByDescending(a => a.StartTs)
                   .AsQueryable();

        if (from.HasValue) q = q.Where(a => a.StartTs >= from.Value);
        if (to.HasValue) q = q.Where(a => a.StartTs < to.Value);

        var alerts = await q.Take(500).ToListAsync(); 
        return View(alerts);
    }
}
