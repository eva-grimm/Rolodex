using Microsoft.EntityFrameworkCore;
using Rolodex.Models;

namespace Rolodex.Services.Interfaces
{
    public interface ICategoryService
    {
        public bool CategoryExists(int id);
        public Task<bool> AddCategoryAsync(Category category);
        public Task<bool> UpdateCategoryAsync(Category category);
        public Task<bool> DeleteCategoryAsync(Category category);
        public Task<IEnumerable<Category>> GetUserCategoriesAsync(string? userId);
        public Task<Category?> GetUserCategoryByIdAsync(int? categoryId, string? userId);
        public Task AddCategoriesToContactAsync(List<int> categoryIds, int contactId);
        public Task RemoveCategoriesFromContactAsync(int contactId);
        public Task AddCategoriesToNoteAsync(List<int> categoryIds, int noteId);
        public Task RemoveCategoriesFromNoteAsync(int noteId);
    }
}
