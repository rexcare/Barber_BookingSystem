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
    public class WorkTimeTemplatesController : Controller
    {
        private readonly AppDbContext _context;

        public WorkTimeTemplatesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/WorkTimeTemplates
        public async Task<IActionResult> Index()
        {
              return _context.WorkTimeTemplates != null ? 
                          View(await _context.WorkTimeTemplates.ToListAsync()) :
                          Problem("Entity set 'AppDbContext.WorkTimeTemplates'  is null.");
        }

        // GET: Admin/WorkTimeTemplates/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.WorkTimeTemplates == null)
            {
                return NotFound();
            }

            var workTimeTemplate = await _context.WorkTimeTemplates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workTimeTemplate == null)
            {
                return NotFound();
            }

            return View(workTimeTemplate);
        }

        // GET: Admin/WorkTimeTemplates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/WorkTimeTemplates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartTime,StopTime,StartTime1,StopTime1,StartTime2,StopTime2,Id")] WorkTimeTemplate workTimeTemplate)
        {
            if (ModelState.IsValid)
            {
                workTimeTemplate.Id = Guid.NewGuid();
                _context.Add(workTimeTemplate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(workTimeTemplate);
        }

        // GET: Admin/WorkTimeTemplates/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.WorkTimeTemplates == null)
            {
                return NotFound();
            }

            var workTimeTemplate = await _context.WorkTimeTemplates.FindAsync(id);
            if (workTimeTemplate == null)
            {
                return NotFound();
            }
            return View(workTimeTemplate);
        }

        // POST: Admin/WorkTimeTemplates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("StartTime,StopTime,StartTime1,StopTime1,StartTime2,StopTime2,Id")] WorkTimeTemplate workTimeTemplate)
        {
            if (id != workTimeTemplate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(workTimeTemplate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkTimeTemplateExists(workTimeTemplate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(workTimeTemplate);
        }

        // GET: Admin/WorkTimeTemplates/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.WorkTimeTemplates == null)
            {
                return NotFound();
            }

            var workTimeTemplate = await _context.WorkTimeTemplates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (workTimeTemplate == null)
            {
                return NotFound();
            }

            return View(workTimeTemplate);
        }

        // POST: Admin/WorkTimeTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.WorkTimeTemplates == null)
            {
                return Problem("Entity set 'AppDbContext.WorkTimeTemplates'  is null.");
            }
            var workTimeTemplate = await _context.WorkTimeTemplates.FindAsync(id);
            if (workTimeTemplate != null)
            {
                _context.WorkTimeTemplates.Remove(workTimeTemplate);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WorkTimeTemplateExists(Guid id)
        {
          return (_context.WorkTimeTemplates?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
