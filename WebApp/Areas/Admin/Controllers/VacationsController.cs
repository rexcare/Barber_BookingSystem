using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using Base.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="admin")]
    public class VacationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public VacationsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Vacations
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users;
            return Redirect("(/../");
        }

        // GET: Admin/Vacations/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Vacations == null)
            {
                return NotFound();
            }

            var vacation = await _context.Vacations
                .Include(v => v.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vacation == null)
            {
                return NotFound();
            }

            return View(vacation);
        }

        // GET: Admin/Vacations/Create
        public async Task<IActionResult> Create(Guid id)
        {
            var user = _userManager.Users.Include(x => x.Vacations).FirstOrDefault(x => x.Id == id);


            if (user!.Vacations.Any())
            {
                var vacations = _context!.Vacations!.First(x => x.AppUserId == user!.Id);
                ViewBag.Vacations = vacations;
            }
            ViewBag.AppUser = user!;
            return View();
        }

        // POST: Admin/Vacations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppUserId,DateStart,DateStop,Id")] Vacation vacation)
        {
            if (ModelState.IsValid)
            {
                vacation.Id = Guid.NewGuid();
                _context.Add(vacation);
                await _context.SaveChangesAsync();
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", vacation.AppUserId);
            var user = _userManager.Users.Include(x => x.Vacations).FirstOrDefault(x => x.Id == vacation.AppUserId);
            var vacations = _context.Vacations.First(x => x.AppUserId == user!.Id);
            ViewBag.AppUser = user!;
            ViewBag.Vacations = vacations!;
            return View(vacation);
        }

        // GET: Admin/Vacations/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Vacations == null)
            {
                return NotFound();
            }

            var vacation = await _context.Vacations.FindAsync(id);
            if (vacation == null)
            {
                return NotFound();
            }
            var user = _userManager.Users.FirstOrDefault(x => x.Id == vacation!.AppUserId);
            ViewBag.AppUser = user!;
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", vacation.AppUserId);
            return View(vacation);
        }

        // POST: Admin/Vacations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AppUserId,DateStart,DateStop,Id")] Vacation vacation)
        {
            if (id != vacation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vacation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VacationExists(vacation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect("../Create/" + vacation!.AppUserId);
            }
            var user = _userManager.Users.FirstOrDefault(x => x.Id == vacation!.AppUserId);
            ViewBag.AppUser = user!;
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", vacation.AppUserId);
            return View(vacation);
        }

        // GET: Admin/Vacations/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Vacations == null)
            {
                return NotFound();
            }

            var vacation = await _context.Vacations
                .Include(v => v.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vacation == null)
            {
                return NotFound();
            }
            var user = _userManager.Users.FirstOrDefault(x => x.Id == vacation.AppUserId);
            ViewBag.AppUser = user!;

            return View(vacation);
        }

        // POST: Admin/Vacations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Vacations == null)
            {
                return Problem("Entity set 'AppDbContext.Vacations'  is null.");
            }
            var vacation = await _context.Vacations.FindAsync(id);
            if (vacation != null)
            {
                _context.Vacations.Remove(vacation);
            }

            var user = _userManager.Users.FirstOrDefault(x => x.Id == vacation!.AppUserId);
            ViewBag.AppUser = user!;
            await _context.SaveChangesAsync();
            return Redirect("../Create/" + vacation!.AppUserId);
        }

        private bool VacationExists(Guid id)
        {
          return (_context.Vacations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
