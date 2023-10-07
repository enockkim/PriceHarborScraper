using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prema.PriceHarborScraper.Models
{

    public class PriceChange
    {
        [Key]
        public int PriceChangeId { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public DateTime ChangeDate { get; set; }

        // Foreign key for the associated product
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
