namespace Rolodex.Services.Interfaces
{
    public interface IAddressBookService
    {
        public Task AddCategoriesToContactAsync(List<int> categoryIds, int contactId);
        public Task RemoveCategoriesFromContactAsync(int contactId);
        public Task UpdateCategoriesAsync(List<int> categoryIds, int contactId);
    }
}
