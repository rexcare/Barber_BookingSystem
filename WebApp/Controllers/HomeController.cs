using System.Diagnostics;
using System.Web;
using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using Base.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using WebApp.Models;
using System.Text.Json;
using System;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly AppDbContext _context;
    
    
    public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
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
    public IActionResult Index()
    {
        // var uuid = User.GetUserId();
        var bal = User.IsInRole("user");
        if (bal) {
            var uuid = User.GetUserId();
            var appointments = _context.Appointments.Where(a=>a.AppUserId.Equals(uuid)).Include(c => c.Customer).Include(s => s.Service);
            ViewData["appointments"] = appointments;
        }
        /*else
        {
            var appointments = _context.Appointments.First();
            ViewData["appointments"] = appointments;
        }*/
        
        var users = _userManager.Users.Where(w=>w.Role=="user").Include(x => x.WorkTimeTemplate);
        CompanyInfo wtt = _context.CompanyInfos.FirstOrDefault(x => x.Id == 1)!;
        ViewBag.WTT = wtt;
        var services = _context.Services.ToList();
        ViewBag.Services = services;
        return View("Index", users);
    }

    public IActionResult Privacy()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
    
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions()
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            }
        );
        return LocalRedirect(returnUrl);
    }

    
    public IActionResult MultiTest()
    {
        var vm = new MultiViewModel();
        
        vm.Tags = new[] {"tag1","tag2","tag3"};
        
        return View(vm);
    }

    [HttpPost]
    public IActionResult MultiTest(MultiViewModel vm)
    {
        return View(vm);
    }

    public async Task<IActionResult> Details(Guid? id)
    {

        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.Users.Include(x => x.WorkTimes).Include(a=>a.Appointments)
            .FirstOrDefaultAsync(m => m.Id == id);
        var appointments = _context.Appointments.Where(c => c.AppUserId.Equals(id)).Include(c => c.Customer).Include(s => s.Service);

        if (user == null)
        {
            return NotFound();
        }

        ViewData["id"] = id;
        ViewData["appointments"] = appointments;

        return View("AppUser/Details",user);
    }
    
    public async Task<IActionResult> GenerateWTT()
    {
        var users =  _userManager.Users.Include(x => x.WorkTimeTemplate).Include(x => x.Vacations).Include(x => x.WorkTimes);
        var companyInfo = _context.CompanyInfos.FirstOrDefault(x => x.Id == 1);

        foreach (var user in users)
        {
            if (user.Email != "admin@itcollege.ee")
            {
                var date = companyInfo!.GenerationDate.Date;
                var vacationsUser = CheckVacations(user);

                for (int i = 0; i < companyInfo!.WorkTimeTemplateInfo; i++)
                {
                    if (!vacationsUser.Contains(date))
                    {
                        var workTime = new WorkTime()
                        {

                            Id = Guid.NewGuid(),
                            AppUserId = user.Id,
                            AppUser = user,
                            Date = date,
                            StartTime = user.WorkTimeTemplate!.StartTime,
                            StopTime = user.WorkTimeTemplate!.StopTime,
                            StartTime1 = user.WorkTimeTemplate!.StartTime1!,
                            StopTime1 = user.WorkTimeTemplate!.StopTime1!,
                            StartTime2 = user.WorkTimeTemplate!.StartTime2!,
                            StopTime2 = user.WorkTimeTemplate!.StopTime2!,

                        };
                        var userWork = user!.WorkTimes.ToList();
                
                        if (!userWork.Any(x => x.Date == workTime.Date))
                        {
                            await _context.WorkTimes.AddAsync(workTime);
                        }
                        if (userWork.Any(x => x.Date == workTime.Date))
                        {
                            var userToDelete = userWork.FirstOrDefault(x => x.Date == workTime.Date);
                            _context.WorkTimes.Remove(userToDelete!);
                            // await this._context.SaveChangesAsync();
                            await this._context.WorkTimes.AddAsync(workTime);
                        }
                    }



                    date = date.AddDays(1);
                }

            }
        }
        
        
        CompanyInfo wtt = _context.CompanyInfos.FirstOrDefault(x => x.Id == 1)!;
        ViewBag.WTT = wtt;
        if (companyInfo != null)
        {
            companyInfo.GenerationDate = companyInfo.GenerationDate.AddDays(companyInfo!.WorkTimeTemplateInfo);
            _context.Update(companyInfo);
        }

        await _context.SaveChangesAsync();
        return View("Index",users);
    }
    
    public async Task<IActionResult> GenerateWttForUser(Guid id)
    {
        var user =  _userManager.Users.Include(x => x.WorkTimeTemplate).Include(x => x.Vacations).Include(x => x.WorkTimes).FirstOrDefault(x => x.Id == id);
        var companyInfo = _context.CompanyInfos.FirstOrDefault(x => x.Id == 1);


        var date = companyInfo!.GenerationDate.Date;
        var vacationsUser = CheckVacations(user);

        for (int i = 0; i < companyInfo!.WorkTimeTemplateInfo; i++)
        {

            if (!vacationsUser.Contains(date))
            {
                var workTime = new WorkTime()
                {

                    Id = Guid.NewGuid(),
                    AppUserId = user.Id,
                    AppUser = user,
                    Date = date,
                    StartTime = user.WorkTimeTemplate!.StartTime,
                    StopTime = user.WorkTimeTemplate!.StopTime,
                    StartTime1 = user.WorkTimeTemplate!.StartTime1!,
                    StopTime1 = user.WorkTimeTemplate!.StopTime1!,
                    StartTime2 = user.WorkTimeTemplate!.StartTime2!,
                    StopTime2 = user.WorkTimeTemplate!.StopTime2!,

                };
                var userWork = user!.WorkTimes!.ToList();
                
                if (!userWork.Any(x => x.Date == workTime.Date))
                {
                    await _context.WorkTimes.AddAsync(workTime);
                }
                if (userWork.Any(x => x.Date == workTime.Date))
                {
                    var userToDelete = userWork.FirstOrDefault(x => x.Date == workTime.Date);
                    _context.WorkTimes.Remove(userToDelete!);
                    // await _context.SaveChangesAsync();
                    await _context.WorkTimes.AddAsync(workTime);
                }
            }



            date = date.AddDays(1);
        }

        
        CompanyInfo wtt = _context.CompanyInfos.FirstOrDefault(x => x.Id == 1)!;
        ViewBag.WTT = wtt;

        ViewData["WorkTimeTemplateId"] = new SelectList(_context.WorkTimeTemplates, "Id", "Times", user.WorkTimeTemplateId);

        ViewBag.WorkTime = user.WorkTimes!;
        await _context.SaveChangesAsync();
        return Redirect("../Edit/" + user!.Id);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public List<DateTime> CheckVacations(AppUser user)
    {

        List<DateTime> vacations = new List<DateTime>();

        foreach (var vac in user.Vacations!)
        {
            DateTime days = vac.DateStart.Date;
            do
            {
                vacations.Add(days);
                days = days.AddDays(1);
            } while (vac.DateStop >= days);
        }

        return vacations;
    }
    
    public async Task<IActionResult> MemberList()
    {

        CompanyInfo wtt = _context.CompanyInfos.FirstOrDefault(x => x.Id == 1)!;
        ViewBag.WTT = wtt;
        var users = _userManager.Users.Where(w=>w.Role == "user").Include(x => x.WorkTimeTemplate);
        return View("AppUser/index", users);
    }
    
    
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.Users.Include(x => x.WorkTimeTemplate).Include(x => x.WorkTimes).Include(a => a.Appointments)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            return NotFound();
        }
        var appointments = _context.Appointments.Where(c => c.AppUserId.Equals(id)).Include(c => c.Customer).Include(s => s.Service);


        ViewData["WorkTimeTemplateId"] = new SelectList(_context.WorkTimeTemplates, "Id", "Times", user.WorkTimeTemplateId);

        ViewData["id"] = id;
        ViewData["appointments"] = appointments;

        ViewBag.WorkTime = user.WorkTimes!;
        CompanyInfo wtt = _context.CompanyInfos.FirstOrDefault(x => x.Id == 1)!;
        ViewBag.WTT = wtt;
        return View("AppUser/Edit",user);
    }
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View("AppUser/Delete",user);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind("Id,FirstName,LastName,Email,PhoneNumber,WorkTimeTemplateId")] AppUser user)
    {
        if (id != user.Id)
        {
            return NotFound();
        }
        
        if (ModelState.IsValid)
        {
            try
            {
                var users = (await _userManager.FindByIdAsync(user.Id.ToString()))!;
                users.FirstName = user.FirstName;
                users.LastName = user.LastName;
                users.Email = user.Email;
                users.PhoneNumber = user.PhoneNumber;
                users.WorkTimeTemplateId = user.WorkTimeTemplateId;
                await _userManager.UpdateAsync(users);

            }
            catch (DbUpdateConcurrencyException)
            {
                return NotFound();
            }
            
            return Redirect("../Edit/" + user.Id);
        }
        ViewData["WorkTimeTemplateId"] = new SelectList(_context.WorkTimeTemplates, "Id", "Times", user.WorkTimeTemplateId);
        return View("AppUser/Edit",user);
    }
    
    
    
    // POST: Admin/WorkTimes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> DeleteWW(Guid? id)
    {

        
        var workTime = await _context.WorkTimes.FindAsync(id);
        if (workTime != null)
        {
            _context.WorkTimes.Remove(workTime);
        }
            
        await _context.SaveChangesAsync();

        return Redirect("../Edit/" + workTime!.AppUserId);
    }
    
    
    
}