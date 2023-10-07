using System.ComponentModel.DataAnnotations;

namespace Prema.PriceHarborScraper.Models
{
    public class Platform
    {
        [Key]
        public int PlatformId { get; set; }

        [Required]
        public string PlatformName { get; set; }
    }
}