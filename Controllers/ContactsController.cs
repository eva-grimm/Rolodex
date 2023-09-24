using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rolodex.Data;
using Rolodex.Enums;
using Rolodex.Models;
using Rolodex.Models.ViewModels;
using Rolodex.Services.Interfaces;

namespace Rolodex.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly ICategoryService _categoryService;
        private readonly IEmailSender _emailService;
        private readonly IContactService _contactService;

        public ContactsController(UserManager<AppUser> userManager,
            IImageService imageManager,
            ICategoryService categoryService,
            IEmailSender emailService,
            IContactService contactService)
        {
            _userManager = userManager;
            _imageService = imageManager;
            _categoryService = categoryService;
            _emailService = emailService;
            _contactService = contactService;
        }

        // GET: Contacts
        public async Task<IActionResult> Index(int? categoryId, string? swalMessage = null)
        {
            // Pass along Sweet Alert message if there is one
            ViewData["SwalMessage"] = swalMessage;

            // get the user's contacts
            string userId = _userManager.GetUserId(User)!;
            List<Contact> contacts = (await _contactService.GetContactsAsync(userId))
                .ToList();

            // and user's categories
            IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);

            // what will contain the filter result
            List<Contact> model = new();

            // filter based on category
            if (categoryId is null) model = contacts;
            else model = categories.SelectMany(c => c.Contacts).ToList();

            // order by last name then first name
            model.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);

            // provide the user's categories
            ViewData["CategoriesList"] = new SelectList(categories, "Id", "Name", categoryId);

            return View(model);
        }

        // GET: Filtered Contacts
        public async Task<IActionResult> SearchContacts(string? searchString)
        {
            // get the user's contacts
            string userId = _userManager.GetUserId(User)!;
            List<Contact> contacts = (await _contactService.GetContactsAsync(userId))
                .ToList();

            // what will contain the search result
            List<Contact> model = new();

            // populate model with results
            if (string.IsNullOrEmpty(searchString)) model = contacts;
            else model = contacts.Where(c => c.FullName!.ToLower().Contains(searchString.ToLower()))
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();

            // provide the user's categories as well
            IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
            ViewData["CategoriesList"] = new MultiSelectList(categories, "Id", "Name");

            // put the user's search into the ViewBag
            ViewData["SearchString"] = searchString;

            return View(nameof(Index), model);
        }

        // GET: Contacts/Create
        public async Task<IActionResult> Create()
        {
            string userId = _userManager.GetUserId(User)!;

            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());

            IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name");

            return View();
        }

        // POST: Contacts/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,DateOfBirth,Address1,Address2,City,State,ZipCode,EmailAddress,PhoneNumber,ImageFile")] Contact contact, List<int> selected)
        {
            ModelState.Remove("AppUserId");

            if (!ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User)!;

                ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());

                IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
                ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name");

                return View();
            }

            try
            {
                // Set User Id
                contact.AppUserId = _userManager.GetUserId(User);

                // Set Created Date
                contact.CreatedDate = DateTime.Now;

                // Set Image data if one has been chosen
                if (contact.ImageFile != null)
                {
                    // Convert file to byte array and assign it to image data
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    // Assign ImageType based on the chosen file
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                await _contactService.AddContactAsync(contact);
                await _categoryService.AddCategoriesToContactAsync(selected, contact.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                string userId = _userManager.GetUserId(User)!;

                ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());

                IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
                ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name");

                return View();
            }
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            Contact? contact = await _contactService.GetContactByIdAsync(id, userId);
            if (contact == null) return NotFound();

            // make Viewdata for states
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());

            // make Viewdata for contact's categories
            IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
            List<int> categoryIds = contact.Categories.Select(c => c.Id).ToList();
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", selectedValues: categoryIds);

            return View(contact);
        }

        // POST: Contacts/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,CreatedDate,DateOfBirth,Address1,Address2,City,State,ZipCode,EmailAddress,PhoneNumber,ImageFile,ImageData,ImageType")] Contact contact, List<int> selected)
        {
            if (id != contact.Id) return NotFound();

            ModelState.Remove("AppUserId");

            if (!ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User)!;

                // make Viewdata for states
                ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());

                // make Viewdata for contact's categories
                IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
                List<int> categoryIds = contact.Categories.Select(c => c.Id).ToList();
                ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", selectedValues: categoryIds);

                return View(contact);
            }
            try
            {
                // Set User Id
                contact.AppUserId = _userManager.GetUserId(User);

                // Set Image data if one has been chosen
                if (contact.ImageFile != null)
                {
                    // Convert file to byte array and assign it to image data
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    // Assign ImageType based on the chosen file
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                await _contactService.UpdateContactAsync(contact);
                await _categoryService.RemoveCategoriesFromContactAsync(contact.Id);
                await _categoryService.AddCategoriesToContactAsync(selected, contact.Id);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            // two people are trying to edit the same category at the same time
            {
                if (!_contactService.ContactExists(contact.Id)) return NotFound();
                else
                {
                    string userId = _userManager.GetUserId(User)!;

                    // make Viewdata for states
                    ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());

                    // make Viewdata for contact's categories
                    IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
                    List<int> categoryIds = contact.Categories.Select(c => c.Id).ToList();
                    ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", selectedValues: categoryIds);

                    return View(contact);
                }
            }
            catch (Exception)
            {
                string userId = _userManager.GetUserId(User)!;

                // make Viewdata for states
                ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());

                // make Viewdata for contact's categories
                IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
                List<int> categoryIds = contact.Categories.Select(c => c.Id).ToList();
                ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", selectedValues: categoryIds);

                return View(contact);
            }
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            Contact? contact = await _contactService.GetContactByIdAsync(id, userId);
            if (contact == null) return NotFound();

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string userId = _userManager.GetUserId(User)!;

            Contact? contact = await _contactService.GetContactByIdAsync(id, userId);

            // if user is attempting to delete a contact that isn't theirs, return not found
            // otherwise, if the contact exists and is theirs, delete
            if (contact != null && contact.AppUserId != userId) return NotFound();
            else if (contact != null) await _contactService.DeleteContactAsync(contact);

            return RedirectToAction(nameof(Index));
        }

        // GET: Contacts/EmailContact/5
        public async Task<IActionResult> EmailContact(int? id, string? swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;

            if (id is null) return NotFound();

            string userId = _userManager.GetUserId(User)!;
            
            // Get the first Contact with Id matching id that belongs to the logged in user
            // else return default
            Contact? contact = await _contactService.GetContactByIdAsync(id, userId);

            if (contact == null) return NotFound();

            EmailData emailData = new()
            {
                EmailAddress = contact!.EmailAddress,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
            };

            EmailContactViewModel viewModel = new()
            {
                Contact = contact,
                EmailData = emailData,
            };

            return View(viewModel);
        }

        //POST: Contacts/EmailContact/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailContact(EmailContactViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string? email = viewModel.EmailData?.EmailAddress;
                    string? subject = viewModel.EmailData?.EmailSubject;
                    string? htmlMessage = viewModel.EmailData?.EmailBody;
                    await _emailService.SendEmailAsync(email!, subject!, htmlMessage!);

                    // Send SweetAlert for success
                    string? swalMessage = "Success: Email sent!";

                    return RedirectToAction(nameof(Index), new { swalMessage = swalMessage });
                }
                catch (Exception)
                {
                    // Send SweetAlert for failure
                    string? swalMessage = "Error: Email failed to send!";

                    return RedirectToAction(nameof(EmailContact), new { swalMessage = swalMessage });
                    throw;
                }
            }
            return View(viewModel);
        }
    }
}