using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Rolodex.Services;
using Rolodex.Services.Interfaces;

namespace Rolodex.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;
        private readonly IEmailSender _emailService;

        public ContactsController(ApplicationDbContext context, 
            UserManager<AppUser> userManager,
            IImageService imageManager, 
            IAddressBookService addressBookService, IEmailSender emailService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageManager;
            _addressBookService = addressBookService;
            _emailService = emailService;
        }

        // GET: Contacts
        public async Task<IActionResult> Index(int? categoryId, string? swalMessage = null)
        {
            // Pass along Sweet Alert message if there is one
            ViewData["SwalMessage"] = swalMessage;

            // get the user's contacts
            string userId = _userManager.GetUserId(User)!;
            List<Contact> contacts = await _context.Contacts
                .Where(c => c.AppUserId == userId)
                .Include(c => c.Categories)
                .ToListAsync();

            // what will contain the search result
            List<Contact> model = new();

            // filter based on category
            if (categoryId is null) model = contacts;
            else model = (await _context.Categories
                    .Include(c => c.Contacts)
                    .FirstOrDefaultAsync(c => c.Id == categoryId))!
                    .Contacts.ToList();

            // order by last name then first name
            model.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList();

            // provide the user's categories as well
            List<Category> categories = await _context.Categories
                .Where(c => c.AppUserId == userId).ToListAsync();
            ViewData["CategoriesList"] = new SelectList(categories, "Id", "Name", categoryId);

            return View(model);
        }

        // GET: Filtered Contacts
        public async Task<IActionResult> SearchContacts(string? searchString)
        {
            // get the user's contacts
            string userId = _userManager.GetUserId(User)!;
            List<Contact> contacts = await _context.Contacts
                .Where(c => c.AppUserId == userId)
                .Include(c => c.Categories)
                .ToListAsync();

            // what will contain the search result
            List<Contact> model = new();

            // populate model with results
            if (string.IsNullOrEmpty(searchString)) model = contacts;
            else model = contacts.Where(c => c.FullName!.ToLower().Contains(searchString.ToLower()))
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToList();

            // provide the user's categories as well
            List<Category> categories = await _context.Categories
                .Where(c => c.AppUserId == userId).ToListAsync();
            ViewData["CategoriesList"] = new MultiSelectList(categories, "Id", "Name");

            // put the user's search into the ViewBag
            ViewData["SearchString"] = searchString;

            return View(nameof(Index),model);
        }

        // GET: Contacts/Create
        public async Task<IActionResult> Create()
        {
            string userId = _userManager.GetUserId(User)!;

            List<Category> categories = await _context.Categories
                .Where(c => c.AppUserId == userId).ToListAsync();

            // make Viewdata for states
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());

            // make Viewdata for categories
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name");

            return View();
        }

        // POST: Contacts/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,DateOfBirth,Address1,Address2,City,State,ZipCode,EmailAddress,PhoneNumber,ImageFile")] Contact contact, List<int> selected)
        {
            ModelState.Remove("AppUserId");
            if (ModelState.IsValid)
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

                _context.Add(contact);
                await _context.SaveChangesAsync();
                await _addressBookService.AddCategoriesToContactAsync(selected, contact.Id);
                return RedirectToAction(nameof(Index));
            }
            return View(contact);
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            // Get the first Contact with Id matching id that belongs to the logged in user
            // else return default
            Contact? contact = await _context.Contacts
                .Include(c => c.Categories)
                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == userId);

            if (contact == null)  return NotFound();

            // make List of user's categories for this contact
            List<Category> categories = await _context.Categories
                .Where(c => c.AppUserId == userId).ToListAsync();
            List<int> categoryIds = contact.Categories.Select(c => c.Id).ToList();

            // make Viewdata for states
            ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>());

            // make Viewdata for contact's categories
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", selectedValues: categoryIds);

            return View(contact);
        }

        // POST: Contacts/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,CreatedDate,DateOfBirth,Address1,Address2,City,State,ZipCode,EmailAddress,PhoneNumber,ImageFile,ImageData,ImageType")] Contact contact, List<int> selected)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
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

                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                    await _addressBookService.RemoveCategoriesFromContactAsync(contact.Id);
                    await _addressBookService.AddCategoriesToContactAsync(selected, contact.Id);
                }
                catch (DbUpdateConcurrencyException)
                // two people are trying to edit the same category at the same time
                {
                    if (!ContactExists(contact.Id))  return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(contact);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            Contact? contact = await _context.Contacts
                .Include(c => c.Categories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contact == null) return NotFound();

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contacts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contacts'  is null.");
            }
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }
            
            await _context.SaveChangesAsync();
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
            Contact? contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == userId);

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

                    return RedirectToAction(nameof(Index), new { swalMessage = swalMessage});
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

        private bool ContactExists(int id)
        {
          return (_context.Contacts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
