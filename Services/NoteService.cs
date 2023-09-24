using Microsoft.EntityFrameworkCore;
using Rolodex.Data;
using Rolodex.Models;
using Rolodex.Services.Interfaces;

namespace Rolodex.Services
{
    public class NoteService : INoteService
    {
        private readonly ApplicationDbContext _context;

        public NoteService(ApplicationDbContext context) 
        {
            _context = context;
        }

        public bool NoteExists(int id)
        {
            return (_context.Notes?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<bool> AddNoteAsync(Note? note)
        {
            if (note == null) return false;

            try
            {
                _context.Add(note);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateNoteAsync(Note? note)
        {
            if (note == null) return false;

            try
            {
                _context.Update(note);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteNoteAsync(Note? note)
        {
            if (note == null) return false;

            try
            {
                _context.Remove(note);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<Note>> GetUserNotesAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Note>();

            return await _context.Notes
                .Where(n => userId.Equals(n.AppUserId))
                .Include(n => n.Categories)
                .ToListAsync();
        }

        public async Task<Note?> GetNoteByIdAsync(int? noteId, string? userId)
        {
            if (noteId == null || string.IsNullOrEmpty(userId)) return new Note();

            return await _context.Notes
                .Include(n => n.Categories)
                .FirstOrDefaultAsync(n => n.Id == noteId
                && userId.Equals(n.AppUserId));
        }
    }
}
