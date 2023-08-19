using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rolodex.Data;
using Rolodex.Models;
using Rolodex.Models.ViewModels;
using Rolodex.Services;
using Rolodex.Services.Interfaces;

namespace Rolodex.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAddressBookService _addressBookService;
        private readonly IEmailSender _emailService;

        public CategoriesController(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IAddressBookService addressBookService, IEmailSender emailService)
        {
            _context = context;
            _userManager = userManager;
            _addressBookService = addressBookService;
            _emailService = emailService;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string? swalMessage = null)
        {
            // Pass along Sweet Alert message if there is one
            ViewData["SwalMessage"] = swalMessage;

            // get the user's categories
            string userId = _userManager.GetUserId(User)!;
            IEnumerable<Category> categories = await _context.Categories
                .Where(c => c.AppUserId == userId)
                .ToListAsync();
            return View(categories);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Category category)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                category.AppUserId = _userManager.GetUserId(User);
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            // Get the first Category with Id matching id that belongs to the logged in user
            // else return default
            Category? category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == userId);

            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AppUserId,Name")] Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                // two people are trying to edit the same category at the same time
                {
                    if (!CategoryExists(category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/EmailCategory/5
        public async Task<IActionResult> EmailCategory(int? id, string? swalMessage = null)
        {
            // Pass along Sweet Alert message if there is one
            ViewData["SwalMessage"] = swalMessage;

            if (id is null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            // Get the first Category with Id matching id that belongs to the logged in user
            // else return default
            Category? category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == userId);

            if (category == null) return NotFound();

            // Build list of contacts in that category
            List<Contact> emailContacts = new();
            emailContacts = (await _context.Categories
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Id == id))!
                .Contacts.ToList();

            // build string containing the emails for each of those contacts
            StringBuilder stringBuilder = new();
            foreach (var contact in emailContacts)
            {
                stringBuilder.Append($"{contact.EmailAddress};");
            }
            //// remove final ; then build string
            stringBuilder.Remove(stringBuilder.ToString().Length-1, 1);
            string? emailList = stringBuilder.ToString();

            // populate model
            EmailData model = new()
            {
                GroupName = category.Name,
                EmailAddress = emailList,
            };

            ViewData["EmailContacts"] = emailContacts;
            return View(model);
        }

        // POST: Categories/EmailCategory/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailCategory(EmailData model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string? email = model.EmailAddress!;
                    string? subject = model.EmailSubject;
                    string? htmlMessage = model.EmailBody;
                    await _emailService.SendEmailAsync(email!, subject!, htmlMessage!);

                    // Send SweetAlert for success
                    string? swalMessage = "Success: Email sent!";

                    return RedirectToAction(nameof(Index), new { swalMessage = swalMessage });
                }
                catch (Exception)
                {
                    // Send SweetAlert for failure
                    string? swalMessage = "Error: Email failed to send!";

                    return RedirectToAction(nameof(EmailCategory), new { swalMessage = swalMessage });
                    throw;
                }
            }
            return View(model);
        }
        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
