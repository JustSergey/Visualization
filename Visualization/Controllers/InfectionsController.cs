using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Visualization.Data;
using Visualization.Models;

namespace Visualization.Controllers
{
    public class InfectionsController : Controller
    {
        private readonly VisualizationContext _context;

        public InfectionsController(VisualizationContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dates = _context.Infections
                .Select(i => i.Date)
                .Distinct()
                .OrderBy(d => d);
            return View(await dates.ToListAsync());
        }

        public async Task<IActionResult> List(string date)
        {
            if (!DateTime.TryParse(date, out DateTime dateTime))
                return NotFound();

            var infections = _context.Infections
                .Include(i => i.Region)
                .Where(i => i.Date == dateTime)
                .OrderByDescending(i => i.Infected);

            if (!infections.Any())
                return NotFound();

            return View(await infections.ToListAsync());
        }

        // GET: Infections/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var infection = await _context.Infections
                .Include(i => i.Region)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (infection == null)
            {
                return NotFound();
            }

            return View(infection);
        }

        // GET: Infections/Create
        public IActionResult Create()
        {
            ViewBag.Regions = new SelectList(_context.Regions, "Id", "Title");
            return View();
        }

        // POST: Infections/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,RegionId,Infected,Recovered,Deaths")] Infection infection)
        {
            if (ModelState.IsValid)
            {
                _context.Add(infection);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Regions = new SelectList(_context.Regions, "Id", "Title", infection.RegionId);
            return View(infection);
        }

        // GET: Infections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var infection = await _context.Infections.FindAsync(id);
            if (infection == null)
            {
                return NotFound();
            }
            ViewData["RegionId"] = new SelectList(_context.Regions, "Id", "Id", infection.RegionId);
            return View(infection);
        }

        // POST: Infections/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,RegionId,Infected,Recovered,Deaths")] Infection infection)
        {
            if (id != infection.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(infection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InfectionExists(infection.Id))
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
            ViewData["RegionId"] = new SelectList(_context.Regions, "Id", "Id", infection.RegionId);
            return View(infection);
        }

        // GET: Infections/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var infection = await _context.Infections
                .Include(i => i.Region)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (infection == null)
            {
                return NotFound();
            }

            return View(infection);
        }

        // POST: Infections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var infection = await _context.Infections.FindAsync(id);
            _context.Infections.Remove(infection);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InfectionExists(int id)
        {
            return _context.Infections.Any(e => e.Id == id);
        }
    }
}
