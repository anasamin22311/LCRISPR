using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CRISPR.Data;
using CRISPR.Models;

namespace CRISPR.Controllers
{
    public class PropsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PropsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Props
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Props.Include(p => p.DataSet);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Props/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Props == null)
            {
                return NotFound();
            }

            var prop = await _context.Props
                .Include(p => p.DataSet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prop == null)
            {
                return NotFound();
            }

            return View(prop);
        }

        // GET: Props/Create
        public IActionResult Create()
        {
            ViewData["ModelId"] = new SelectList(_context.DataSets, "id", "id");
            return View();
        }

        // POST: Props/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Value,ModelId")] Prop prop)
        {
            if (ModelState.IsValid)
            {
                _context.Add(prop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ModelId"] = new SelectList(_context.DataSets, "id", "id", prop.ModelId);
            return View(prop);
        }

        // GET: Props/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Props == null)
            {
                return NotFound();
            }

            var prop = await _context.Props.FindAsync(id);
            if (prop == null)
            {
                return NotFound();
            }
            ViewData["ModelId"] = new SelectList(_context.DataSets, "id", "id", prop.ModelId);
            return View(prop);
        }

        // POST: Props/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Value,ModelId")] Prop prop)
        {
            if (id != prop.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropExists(prop.Id))
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
            ViewData["ModelId"] = new SelectList(_context.DataSets, "id", "id", prop.ModelId);
            return View(prop);
        }

        // GET: Props/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Props == null)
            {
                return NotFound();
            }

            var prop = await _context.Props
                .Include(p => p.DataSet)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prop == null)
            {
                return NotFound();
            }

            return View(prop);
        }

        // POST: Props/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Props == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Props'  is null.");
            }
            var prop = await _context.Props.FindAsync(id);
            if (prop != null)
            {
                _context.Props.Remove(prop);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PropExists(int id)
        {
          return (_context.Props?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
