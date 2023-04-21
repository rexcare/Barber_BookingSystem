using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.DAL.EF;
using App.Domain;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyInfosController : Controller
    {
        private readonly AppDbContext _context;

        public CompanyInfosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/CompanyInfos
        public async Task<IActionResult> Index()
        {
              return _context.CompanyInfos != null ? 
                          View(await _context.CompanyInfos.ToListAsync()) :
                          Problem("Entity set 'AppDbContext.CompanyInfos'  is null.");
        }

        // GET: Admin/CompanyInfos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CompanyInfos == null)
            {
                return NotFound();
            }

            var companyInfo = await _context.CompanyInfos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyInfo == null)
            {
                return NotFound();
            }

            return View(companyInfo);
        }

        // GET: Admin/CompanyInfos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/CompanyInfos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Address,Email,Phone,WorkTimes, WorkTimeTemplateInfo,GenerationDate")] CompanyInfo companyInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(companyInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(companyInfo);
        }

        // GET: Admin/CompanyInfos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            id = 1;
            if (id == null || _context.CompanyInfos == null)
            {
                return NotFound();
            }

            var companyInfo = await _context.CompanyInfos.FindAsync(id);
            if (companyInfo == null)
            {
                return NotFound();
            }
            return View(companyInfo);
        }

        // POST: Admin/CompanyInfos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Address,Email,Phone,WorkTimes,WorkTimeTemplateInfo,GenerationDate")] CompanyInfo companyInfo)
        {
            if (id != companyInfo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(companyInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyInfoExists(companyInfo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect("../../");
            }
            return View(companyInfo);
        }

        // GET: Admin/CompanyInfos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CompanyInfos == null)
            {
                return NotFound();
            }

            var companyInfo = await _context.CompanyInfos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (companyInfo == null)
            {
                return NotFound();
            }

            return View(companyInfo);
        }

        // POST: Admin/CompanyInfos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CompanyInfos == null)
            {
                return Problem("Entity set 'AppDbContext.CompanyInfos'  is null.");
            }
            var companyInfo = await _context.CompanyInfos.FindAsync(id);
            if (companyInfo != null)
            {
                _context.CompanyInfos.Remove(companyInfo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyInfoExists(int id)
        {
          return (_context.CompanyInfos?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
