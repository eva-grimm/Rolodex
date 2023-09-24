using Rolodex.Models;

namespace Rolodex.Services.Interfaces
{
    public interface IContactService
    {
        public bool ContactExists(int id);
        public Task<bool> AddContactAsync(Contact? contact);
        public Task<bool> UpdateContactAsync(Contact? contact);
        public Task<bool> DeleteContactAsync(Contact? contact);
        public Task<IEnumerable<Contact>> GetContactsAsync(string? userId);
        public Task<Contact?> GetContactByIdAsync(int? contactId, string? userId);
    }
}
