using Rolodex.Models;
namespace Rolodex.Services.Interfaces
{
    public interface INoteService
    {
        public bool NoteExists(int id);
        public Task<bool> AddNoteAsync(Note? note);
        public Task<bool> UpdateNoteAsync (Note? note);
        public Task<bool> DeleteNoteAsync(Note? note);
        public Task<IEnumerable<Note>> GetUserNotesAsync(string? userId);
        public Task<Note?> GetNoteByIdAsync(int? noteId, string? userId);
    }
}
