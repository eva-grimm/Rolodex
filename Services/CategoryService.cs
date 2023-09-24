using Microsoft.EntityFrameworkCore;
using Rolodex.Data;
using Rolodex.Models;
using Rolodex.Services.Interfaces;

namespace Rolodex.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            if (category == null) return false;

            try
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            if (category == null) return false;

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(Category category)
        {
            if (category == null) return false;

            try
            {
                _context.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Category>();

            return await _context.Categories
                .Where(c => userId.Equals(c.AppUserId))
                .Include(c => c.Contacts)
                .Include(c => c.Notes)
                .ToListAsync();
        }

        public async Task<Category?> GetUserCategoryByIdAsync(int? categoryId, string? userId)
        {
            if (categoryId == null || string.IsNullOrEmpty(userId)) return new Category();

            return await _context.Categories
                .Include(c => c.Contacts)
                .Include(c => c.Notes)
                .FirstOrDefaultAsync(c => c.Id == categoryId
                    && userId.Equals(c.AppUserId));
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

                foreach (int categoryId in categoryIds)
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

        public async Task AddCategoriesToNoteAsync(List<int> categoryIds, int noteId)
        {
            try
            {
                Note? note = await _context.Notes
                    .Include(c => c.Categories)
                    .FirstOrDefaultAsync(c => c.Id == noteId);
                if (note is null) return;

                foreach (int categoryId in categoryIds)
                {
                    Category? category = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Id == categoryId);
                    if (category is not null) note.Categories.Add(category);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveCategoriesFromNoteAsync(int noteId)
        {
            try
            {
                Note? note = await _context.Notes
                    .Include(c => c.Categories)
                    .FirstOrDefaultAsync(c => c.Id == noteId);
                if (note is not null)
                {
                    note.Categories.Clear();
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
