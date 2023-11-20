using System.ComponentModel.DataAnnotations;

namespace Prema.PriceHarbor.Scraper.Models
{
    public class Platform
    {
        [Key]
        public int PlatformId { get; set; }

        [Required]
        public string PlatformName { get; set; }
    }
}