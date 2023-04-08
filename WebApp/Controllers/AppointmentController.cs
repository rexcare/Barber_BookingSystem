using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers;

/// <summary>
/// 
/// </summary>
public class AppointmentController : Controller
{
    private readonly ILogger<AppointmentController> _logger;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly AppDbContext _context;
    
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="context"></param>
    /// <param name="userManager"></param>
    /// <param name="signInManager"></param>
    /// <param name="roleManager"></param>
    public AppointmentController(ILogger<AppointmentController> logger, AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;

    }   
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        Console.WriteLine("services");
        return _context.Services != null ? 
            View(await _context.Services.ToListAsync()) :
            Problem("Entity set 'AppDbContext.Services'  is null.");
    }

    public async Task<IActionResult> Booking(Guid? id)
    {
        if (id == null || _context.Services == null)
        {
            return NotFound();
        }

        var service = await _context.Services.FindAsync(id);
        if (service == null)
        {
            return NotFound();
        }
        Console.WriteLine(service);
        ViewData["service"] = service;
        return _context.WorkTimes != null ?
            View() :
            Problem("Entity set 'AppDbContext.workTimes'  is null.");
    }

    public IActionResult makeAppointment(string Id, string Address)
    {
        Console.WriteLine(Id);
        return Json(Id + Address);
    }


}