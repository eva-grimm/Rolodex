using System.ComponentModel.DataAnnotations;

namespace Rolodex.Models
{
    public class Category
    {
        // Primary Key
        public int Id { get; set; }
        // Foreign Keys
        [Required]
        public string? AppUserId { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        public string? Name { get; set; }

        // Navigation Properties
        public virtual AppUser? AppUser { get; set; }
        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
        public virtual ICollection<Note> Notes { get; set; } = new HashSet<Note>();
    }
}
