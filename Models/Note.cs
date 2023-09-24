using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Rolodex.Models
{
    public class Note
    {
        private DateTime _created;
        private DateTime? _updated;

        // Primary Key
        public int Id { get; set; }

        // Properties
        [DataType(DataType.Date)]
        public DateTime Created
        {
            get => _created.ToLocalTime();
            set => _created = value.ToUniversalTime();
        }

        [DataType(DataType.Date)]
        public DateTime? Updated
        {
            get => _updated?.ToLocalTime();
            set => _updated = value.HasValue ? value.Value.ToUniversalTime() : null;
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
