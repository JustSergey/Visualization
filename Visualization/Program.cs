using System;
using System.Collections.Generic;
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
            {
                var services = scope.ServiceProvider;
                InitializeDataBase(services);
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void InitializeDataBase(IServiceProvider serviceProvider)
        {
            using (var context = new VisualizationContext(
                serviceProvider.GetRequiredService<DbContextOptions<VisualizationContext>>()))
            {
                if (context.Infections.Any())
                    return;
                if (!File.Exists("initdata.txt"))
                    return;

                string[] lines = File.ReadAllLines("initdata.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] line = lines[i].Split('\t');
                    string regionTitle = line[1];
                    Region region = context.Regions.FirstOrDefault(r => r.Title == regionTitle);
                    if (region == default)
                    {
                        region = new Region() { Title = regionTitle };
                        context.Regions.Add(region);
                    }
                    DateTime date = DateTime.Parse(line[0]);
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
}
