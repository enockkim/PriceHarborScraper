using System.ComponentModel.DataAnnotations;

namespace Prema.PriceHarborScraper.Models
{
    public class Manufacturer
    {
        [Key]
        public int ManufacturerId { get; set; }

        [Required]
        public string ManufacturerName { get; set; }
    }
}