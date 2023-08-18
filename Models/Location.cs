using Rolodex.Enums;
using System.ComponentModel.DataAnnotations;

namespace Rolodex.Models
{
    public class Location
    {
        // Primary Key
        public int Id { get; set; }

        // Foreign Keys
        [Required]
        public List<string>? AppUserIds { get; set; }
        public List<string>? Contacts { get; set; }

        // Address
        [Display(Name = "Street Address")]
        public string? Address1 { get; set; }
        [Display(Name = "Stress Address Line 2")]
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public States? State { get; set; }
        [Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public int? ZipCode { get; set; }

        // Geocode
        public double longitude { get; set; }
        public double latitude { get; set; }
    }
}
