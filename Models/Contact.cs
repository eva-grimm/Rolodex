using Rolodex.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rolodex.Models
{
    public class Contact
    {
        // Primary Key
        public int Id { get; set; }


        // Foreign Key
        [Required]
        public string? AppUserId { get; set; }


        // Name
        [Required, Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} character long.", MinimumLength = 2)]
        public string? FirstName { get; set; }
        [Required, Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} character long.", MinimumLength = 2)]
        public string? LastName { get; set; }
        [NotMapped]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }


        // Dates
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }
        [DataType(DataType.Date), Display(Name = "Birthday")]
        public DateTime? DateOfBirth { get; set; }


        // Address
        [Display(Name = "Street Address")]
        public string? Address1 { get; set; }
        [Display(Name = "Stress Address Line 2")]
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public States? States { get; set; }
        [Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public int? ZipCode { get; set; }


        // Contact Info
        [Required, Display(Name = "Email Address")]
        [DataType(DataType.EmailAddress)]
        public string? EmailAddress { get; set; }
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }


        // Image
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }


        // Navigation Properties - refers to EntityFramework
        public virtual AppUser? AppUser { get; set; }
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    }
}