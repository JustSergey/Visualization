using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Visualization.Data;
using Visualization.Models;

namespace Visualization.Controllers
{
    public class InfectionsController : Controller
    {
        public class Increase
        {
            public int Id;
            public int Inf;
            public int Infc;
            public int Rec;
            public int Dea;
        }

        public class GroupedIncreases
        {
            public DateTime Date;
            public List<Increase> Increases;
        }

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
            return View(await GetDates());
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

        public async Task<IActionResult> Map()
        {
            var dates = await GetDates();
            var increases = GetIncreases();
            var averageIncreases = GetAverageIncreases(increases);
            var regions = _context.Regions
                .AsEnumerable()
                .OrderByDescending(r => averageIncreases[r.Id - 1].Inf);
            return View(new MapData() { Dates = dates, Regions = regions.ToList() });
        }

        private async Task<List<DateTime>> GetDates()
        {
            var dates = _context.Infections
                .Select(i => i.Date)
                .Distinct()
                .OrderBy(d => d);
            return await dates.ToListAsync();
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

        [HttpPost]
        public JsonResult GetIncrease(string type)
        {
            var increases = GetIncreases();
            if (type == "daily")
                return Json(increases.Select(i => new
                {
                    i.Date,
                    Increases = i.Increases.Select(j => new
                    {
                        j.Id,
                        j.Inf,
                        j.Infc,
                        j.Rec,
                        j.Dea
                    })
                }));
            else if (type == "average")
            {
                var averageIncreases = GetAverageIncreases(increases);
                return Json(averageIncreases.Select(i => new
                {
                    i.Id,
                    i.Inf,
                    i.Rec,
                    i.Dea
                }));
            }
            else
                return null;
        }

        private List<Increase> GetAverageIncreases(List<GroupedIncreases> increases)
        {
            int[] ave_inf = new int[90];
            int[] ave_rec = new int[90];
            int[] ave_dea = new int[90];
            foreach (var inc in increases)
            {
                foreach (var reg in inc.Increases)
                {
                    ave_inf[reg.Id] += reg.Inf;
                    ave_rec[reg.Id] += reg.Rec;
                    ave_dea[reg.Id] += reg.Dea;
                }
            }
            for (int i = 0; i < 90; i++)
            {
                ave_inf[i] /= increases.Count;
                ave_rec[i] /= increases.Count;
                ave_dea[i] /= increases.Count;
            }
            var result_increases = _context.Regions
                .OrderBy(r => r.Id)
                .AsEnumerable()
                .Select(r => new Increase
                {
                    Id = r.Id,
                    Inf = ave_inf[r.Id],
                    Rec = ave_rec[r.Id],
                    Dea = ave_dea[r.Id]
                }).ToList();
            return result_increases;
        }

        private List<GroupedIncreases> GetIncreases()
        {
            var infections = _context.Infections
                .OrderBy(i => i.Date)
                .AsEnumerable()
                .GroupBy(i => i.Date)
                .ToList();
            int[,] inf = new int[90, infections.Count()];
            int[,] infc = new int[90, infections.Count()];
            int[,] rec = new int[90, infections.Count()];
            int[,] dea = new int[90, infections.Count()];
            for (int i = 1; i < infections.Count(); i++)
            {
                foreach (var infec_next in infections[i])
                {
                    foreach (var infec_prev in infections[i - 1])
                    {
                        if (infec_prev.RegionId == infec_next.RegionId)
                        {
                            if (infec_next.Infected != 0 && infec_prev.Infected != 0)
                            {
                                inf[infec_prev.RegionId, i] = (int)Math.Round((float)(infec_next.Infected - infec_prev.Infected) / infec_prev.Infected * 100);
                                infc[infec_prev.RegionId, i] = infec_next.Infected - infec_prev.Infected;
                            }
                            if (infec_next.Recovered != 0 && infec_prev.Recovered != 0)
                                rec[infec_prev.RegionId, i] = (int)Math.Round((float)(infec_next.Recovered - infec_prev.Recovered) / infec_prev.Recovered * 100);
                            if (infec_next.Deaths != 0 && infec_prev.Deaths != 0)
                                dea[infec_prev.RegionId, i] = (int)Math.Round((float)(infec_next.Deaths - infec_prev.Deaths) / infec_prev.Deaths * 100);
                        }
                    }
                }
            }
            DateTime begin = infections[0].Key;
            var increases = infections
                .Select(i => new GroupedIncreases
                {
                    Date = i.Key,
                    Increases = i.Select(r => new Increase
                    {
                        Id = r.RegionId,
                        Inf = inf[r.RegionId, (i.Key - begin).Days],
                        Infc = infc[r.RegionId, (i.Key - begin).Days],
                        Rec = rec[r.RegionId, (i.Key - begin).Days],
                        Dea = dea[r.RegionId, (i.Key - begin).Days]
                    }).OrderBy(r => r.Id).ToList()
                });
            return increases.ToList();
        }
    }
}
