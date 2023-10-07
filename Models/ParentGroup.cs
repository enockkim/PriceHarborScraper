using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prema.PriceHarborScraper.Models
{
    public class ParentGroup
    {
        [Key]
        public int ProductGroupId { get; set; }

        [Required]
        public string ParentGroupName { get; set; }

        // Navigation property for ProductGroups within the parent
        public ICollection<ProductGroup> ProductGroups { get; set; }
    }
}
