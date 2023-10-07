using Prema.PriceHarborScraper.Models;
using Microsoft.EntityFrameworkCore;
using Prema.PriceHarborScraper.Models;
using System.Collections.Generic;

namespace FourtitudeIntegrated.DbContexts
{
    public class PriceHarborContext : DbContext
    {
        public DbSet<Product> Product { set; get; }
        public DbSet<ProductGroup> ProductGroup { set; get; }
        public DbSet<Manufacturer> Manufacturer { set; get; }
        public DbSet<Platform> Platform { set; get; }
        public DbSet<PriceChange> PriceChange { set; get; }
        public DbSet<ParentGroup> ParentGroup { set; get; }
        public PriceHarborContext(DbContextOptions<PriceHarborContext> options) : base(options)
        {

        }
    }
}
