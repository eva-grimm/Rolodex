using Microsoft.EntityFrameworkCore;
using Rolodex.Data;
using Rolodex.Models;
using Rolodex.Services.Interfaces;

namespace Rolodex.Services
{
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _context;

        public ContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool ContactExists(int id)
        {
            return (_context.Contacts?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<bool> AddContactAsync(Contact? contact)
        {
            if (contact == null) return false;

            try
            {
                _context.Add(contact);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateContactAsync(Contact? contact)
        {
            if (contact == null) return false;

            try
            {
                _context.Update(contact);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteContactAsync(Contact? contact)
        {
            if (contact == null) return false;

            try
            {
                _context.Remove(contact);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<Contact>> GetContactsAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Contact>();

            return await _context.Contacts
                .Where(c => userId.Equals(c.AppUserId))
                .Include(c => c.Categories)
                .ToListAsync();
        }

        public async Task<Contact?> GetContactByIdAsync(int? contactId, string? userId)
        {
            if (contactId == null || string.IsNullOrEmpty(userId)) return new Contact();

            return await _context.Contacts
                .Include(c => c.Categories)
                .FirstOrDefaultAsync(c => c.Id == contactId
                && userId.Equals(c.AppUserId));
        }

    }
}
