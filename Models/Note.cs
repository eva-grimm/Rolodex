using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rolodex.Models
{
    public class Note
    {
        private DateTime _createdDate;

        // Primary Key
        public int Id { get; set; }

        // Properties
        [DataType(DataType.Date)]
        public DateTime CreatedDate
        {
            get => _createdDate.ToLocalTime();
            set => _createdDate = value.ToUniversalTime();
        }

        public string? NoteTitle { get; set; }
        public string? NoteText { get; set; }

        [NotMapped]
        public IFormFile? AudioFile { get; set; }
        public byte[]? AudioData { get; set; }
        public string? AudioType { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        // Nav
        [Required]
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
        public ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    }
}
