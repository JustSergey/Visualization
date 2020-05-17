using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dates()
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

        public Task<IActionResult> Map()
        {
            return Dates();
        }

        [HttpPost]
        public JsonResult GetInfections(string type)
        {
            if (type == "infections")
            {
                var infections = _context.Infections
                    .OrderBy(i => i.Date)
                    .AsEnumerable()
                    .GroupBy(i => i.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Infections = g
                            .Select(i => new
                            {
                                Region = i.RegionId,
                                i.Infected,
                                i.Recovered,
                                i.Deaths
                            }).OrderBy(i => i.Region)
                    });
                return Json(infections);
            }
            else if (type == "regions")
            {
                var regions = _context.Regions
                    .OrderBy(r => r.Id)
                    .AsEnumerable();
                return Json(regions);
            }
            else
                return null;
        }
    }
}
