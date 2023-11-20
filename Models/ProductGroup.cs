using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prema.PriceHarbor.Scraper.Models
{
    public class ProductGroup
    {
        [Key]
        public int ProductGroupId { get; set; }

        [Required]
        public string ProductGroupName { get; set; }

        // Navigation property for products within the group
        public ICollection<Product> Products { get; set; }

        // Foreign key for the parent group (if applicable)
        public int? ParentGroupId { get; set; }

        [ForeignKey("ParentGroupId")]
        public ParentGroup ParentGroup { get; set; }
    }
}
