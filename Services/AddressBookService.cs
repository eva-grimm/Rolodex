using Microsoft.EntityFrameworkCore;
using Rolodex.Data;
using Rolodex.Models;
using Rolodex.Services.Interfaces;

namespace Rolodex.Services
{
    public class AddressBookService : IAddressBookService
    {
        private readonly ApplicationDbContext _context;
        public AddressBookService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddCategoriesToContactAsync(List<int> categoryIds, int contactId)
        {
            try
            {
                // get contact to add categories to
                Contact? contact = await _context.Contacts
                    .Include(c => c.Categories)
                    .FirstOrDefaultAsync(c => c.Id == contactId);

                // if this contact doesn't exist, stop
                if (contact is null) return;

                foreach(int categoryId in categoryIds)
                {
                    // make sure category exists
                    Category? category = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Id == categoryId);

                    // if it does, add the contact to that category
                    if (category is not null) contact.Categories.Add(category);
                }

                // save changes to database
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveCategoriesFromContactAsync(int contactId)
        {
            try
            {
                // get contact to remove categories from
                Contact? contact = await _context.Contacts
                    .Include(c => c.Categories)
                    .FirstOrDefaultAsync(c => c.Id == contactId);

                if (contact is not null)
                {
                    // remove all of their categories
                    contact.Categories.Clear();

                    // save those changes to the database
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task UpdateCategoriesAsync(List<int> categoryIds, int contactId)
        {
            throw new NotImplementedException();
        }
    }
}
