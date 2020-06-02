using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Visualization.Models;

namespace Visualization.Data
{
    public class VisualizationContext : DbContext
    {
        public VisualizationContext(DbContextOptions<VisualizationContext> options) : base(options) { }

        public DbSet<Region> Regions { get; set; }
        public DbSet<Infection> Infections { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Region>().ToTable("Region");
            modelBuilder.Entity<Infection>().ToTable("Infection");
        }
    }
}
