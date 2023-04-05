using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="admin")]
    public class WorkTimesController : Controller
    {
        private readonly AppDbContext _context;

        public WorkTimesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/WorkTimes
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.WorkTimes.Include(w => w.AppUser);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Admin/WorkTimes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.WorkTimes == null)
            {
                return NotFound();
            }

            var workTime = await _context.WorkTimes
                .Include(w => w.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workTime == null)
            {
                return NotFound();
            }

            return View(workTime);
        }

        // GET: Admin/WorkTimes/Create
        public IActionResult Create(Guid? id)
        {
            // ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName");
            ViewBag.AppUserId = id;
            return View();
        }

        // POST: Admin/WorkTimes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppUserId,Date,StartTime,StopTime,StartTime1,StopTime1,StartTime2,StopTime2,Id")] WorkTime workTime)
        {
            if (ModelState.IsValid)
            {
                workTime.Id = Guid.NewGuid();
                _context.Add(workTime);
                await _context.SaveChangesAsync();
                return Redirect("~/Home/Edit/" + workTime.AppUserId);
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", workTime.AppUserId);
            return View(workTime);
        }

        // GET: Admin/WorkTimes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.WorkTimes == null)
            {
                return NotFound();
            }

            var workTime = await _context.WorkTimes.FindAsync(id);
            if (workTime == null)
            {
                return NotFound();
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", workTime.AppUserId);
            return View(workTime);
        }

        // POST: Admin/WorkTimes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AppUserId,Date,StartTime,StopTime,StartTime1,StopTime1,StartTime2,StopTime2,Id")] WorkTime workTime)
        {
            if (id != workTime.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workTime);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkTimeExists(workTime.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect("../../../Home/Edit/" + workTime.AppUserId);
            }
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "FirstName", workTime.AppUserId);
            return View(workTime);
        }

        // GET: Admin/WorkTimes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.WorkTimes == null)
            {
                return NotFound();
            }

            var workTime = await _context.WorkTimes
                .Include(w => w.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workTime == null)
            {
                return NotFound();
            }

            return View(workTime);
        }

        // POST: Admin/WorkTimes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.WorkTimes == null)
            {
                return Problem("Entity set 'AppDbContext.WorkTimes'  is null.");
            }
            var workTime = await _context.WorkTimes.FindAsync(id);
            if (workTime != null)
            {
                _context.WorkTimes.Remove(workTime);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkTimeExists(Guid id)
        {
          return (_context.WorkTimes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
