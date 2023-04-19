using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using Base.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Protocol;
using System;
using System.Text.Json;

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

        var service = await _context.Services.FirstOrDefaultAsync(m => m.Id == id);
        if (service == null)
        {
            return NotFound();
        }
        var barbers = _userManager.Users.Where(x => x.Role =="user").Include(w=>w.WorkTimes);
        ViewData["service"] = service;
        ViewData["barber"] = barbers;
        return View();
    }

    public async Task<IActionResult> CreateCustomer([Bind("FirstName,LastName,Email,Phone")] Customer customer)
    {
        if (ModelState.IsValid)
        {
            var oldCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == customer.Email && c.Phone == customer.Phone);
            if(oldCustomer != null)
                return Json(oldCustomer.Id);
            customer.Id = Guid.NewGuid();
            _context.Add(customer);
            await _context.SaveChangesAsync();
            return Json(customer.Id);
        }
        // return RedirectToAction(nameof(Index));
        return Json("error");
    }

    public async Task<IActionResult> BookPlan(Guid? id)
    {
        if (id == null || _context.Appointments == null)
        {
            return NotFound();
        }

        var appointments = _context.Appointments
                .Where(c => c.AppUserId.Equals(id))
                .Select(p => p.StartTime);

        return Json(await appointments.ToListAsync());
    }

    public async Task<IActionResult> BookList(Guid? id)
    {
        if (id == null || _context.Appointments == null)
        {
            return NotFound();
        }

        var user = _userManager.Users.First(u => u.Id == id);
        if (user== null) return NotFound();
        var customer = _context.Customers.First(f => f.Email == user.Email);
        if (customer == null) return NotFound();
        var appointments = _context.Appointments.Where(c => c.CustomerId.Equals(customer.Id)).Include(w => w.Service).Include(w => w.AppUser);
        return View(await appointments.ToListAsync());
    }

    public async Task<IActionResult> readAppointmentsById(Guid? id)
    {
        if (id == null || _context.Appointments == null)
        {
            return NotFound();
        }

        var appointments = _context.Appointments.Where(c => c.AppUserId.Equals(id)).Include(c=>c.Customer).Include(s=>s.Service).OrderBy(d=>d.StartTime);
        
        var jsonString = JsonConvert.SerializeObject(
                            appointments, 
                            Formatting.None,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            }
        );

        return Json(jsonString);
    }

    public async Task<IActionResult> CreateAppointment([Bind("CustomerId,ServiceId,AppUserId,StartTime")] Appointment Appointment)
    {
        if (ModelState.IsValid)
        {
            Appointment.Id = Guid.NewGuid();
            _context.Add(Appointment);
            Console.WriteLine(Appointment.StartTime);
            await _context.SaveChangesAsync();
            return Json(Appointment.Id);
        }
        // return RedirectToAction(nameof(Index));
        return Json("error");
    }

    public async Task<IActionResult> DeleteAppointment(Guid? id, Guid? uid)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
        }

        await _context.SaveChangesAsync();
        // return RedirectToAction(nameof(Index));
        return Json(uid);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var appointment = _context.Appointments.Where(c => c.Id.Equals(id)).Include(c => c.Customer).Include(s => s.Service).First();
        ViewData["appointment"] = appointment;
        var barbers = _userManager.Users.Where(x => x.Role == "user").Include(w => w.WorkTimes);
        ViewData["barber"] = barbers;

        return View();
    }

    public async Task<IActionResult> UpdateAppointment([Bind("Id,ServiceId,AppUserId,StartTime,CustomerId")] Appointment appointment)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(appointment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Json("success");
        }
        return Json("error");
    }

    private bool AppointmentExists(Guid id)
    {
        return (_context.Appointments?.Any(e => e.Id == id)).GetValueOrDefault();
    }

    public async Task<IActionResult> getWorkingTimes(Guid? id)
    {
        if (id == null || _context.WorkTimes == null)
        {
            return NotFound();
        }
        var workTime = _context.WorkTimes
                .Where(c => c.AppUserId.Equals(id))
                .OrderBy(d=>d.Date);
        if (workTime == null)
        {
            return NotFound();
        }

        return Json(await workTime.ToListAsync());
    }
}