using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prema.PriceHarborScraper.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Link { get; set; }

        // Foreign key to reference the manufacturer ie samsung, lg, von etc
        public int ManufacturerId { get; set; }

        [ForeignKey("ManufacturerId")]
        public Manufacturer Manufacturer { get; set; }

        // Foreign key to reference the platform ie jumia, kilimall etc
        public int PlatformId { get; set; }

        [ForeignKey("PlatformId")]
        public Platform Platform { get; set; }

        // Navigation property to link products to their price history
        public ICollection<PriceChange> PriceChanges { get; set; }

        // Foreign key for the product's group
        public int ProductGroupId { get; set; }

        [ForeignKey("ProductGroupId")]
        public ProductGroup ProductGroup { get; set; }
    }

}
