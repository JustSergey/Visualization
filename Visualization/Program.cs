using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Visualization.Data;
using Visualization.Models;

namespace Visualization
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())

                CreateDbIfNotExists(host);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context = services.GetRequiredService<VisualizationContext>();
                    InitializeDataBase(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }
        }

        private static void InitializeDataBase(VisualizationContext context)
        {
            context.Database.EnsureCreated();

            if (context.Infections.Any() || context.Regions.Any())
                return;
            if (!File.Exists("initdata.txt"))
                return;
            if (!File.Exists("regions.txt"))
                return;

            List<Region> regions = new List<Region>();
            string[] regLines = File.ReadAllLines("regions.txt");
            for (int i = 0; i < regLines.Length; i++)
            {
                string[] line = regLines[i].Split('\t');
                Region region = new Region()
                {
                    Id = int.Parse(line[0]),
                    Title = line[1]
                };
                regions.Add(region);
                context.Regions.Add(region);
            }
            context.SaveChanges();

            string[] lines = File.ReadAllLines("initdata.txt");
            CultureInfo provider = CultureInfo.InvariantCulture;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] line = lines[i].Split('\t');
                DateTime date = DateTime.ParseExact(line[0], "dd.MM.yyyy", provider);
                string regionTitle = line[1];
                Region region = regions.Find(r => r.Title == regionTitle);
                //if (region == default)
                //{
                //    region = new Region() { Title = regionTitle };
                //    regions.Add(region);
                //    context.Regions.Add(region);
                //}
                int infected = int.Parse(line[2]);
                int recovered = int.Parse(line[3]);
                int deaths = int.Parse(line[4]);
                Infection infection = new Infection()
                {
                    Date = date,
                    Region = region,
                    Infected = infected,
                    Recovered = recovered,
                    Deaths = deaths
                };
                context.Infections.Add(infection);
            }
            context.SaveChanges();
        }
    }
}