namespace Rolodex.Services.Interfaces
{
    public interface ICategoryService
    {
        public Task AddCategoriesToContactAsync(List<int> categoryIds, int contactId);
        public Task RemoveCategoriesFromContactAsync(int contactId);
        public Task AddCategoriesToNoteAsync(List<int> categoryIds, int noteId);
        public Task RemoveCategoriesFromNoteAsync(int noteId);
    }
}
