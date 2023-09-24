using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rolodex.Data;
using Rolodex.Models;
using Rolodex.Services.Interfaces;

namespace Rolodex.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailService;
        private readonly ICategoryService _categoryService;

        public CategoriesController(UserManager<AppUser> userManager,
            IEmailSender emailService,
            ICategoryService categoryService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _categoryService = categoryService;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string? swalMessage = null)
        {
            // Pass along Sweet Alert message if there is one
            ViewData["SwalMessage"] = swalMessage;

            // get the user's categories
            string userId = _userManager.GetUserId(User)!;
            IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
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

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            try
            {
                category.AppUserId = _userManager.GetUserId(User);
                await _categoryService.AddCategoryAsync(category);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return View(category);
            }
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            Category? category = await _categoryService.GetUserCategoryByIdAsync(id, userId);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AppUserId,Name")] Category category)
        {
            if (id != category.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(category);
            }
            try
            {
                await _categoryService.UpdateCategoryAsync(category);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            // two people are trying to edit the same category at the same time
            {
                if (!_categoryService.CategoryExists(category.Id)) return NotFound();
                else return View(category);
            }
            catch (Exception)
            {
                return View(category);
            }
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            Category? category = await _categoryService.GetUserCategoryByIdAsync(id, userId);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string userId = _userManager.GetUserId(User)!;

            Category? category = await _categoryService.GetUserCategoryByIdAsync(id, userId);

            // if user is attempting to delete a category that isn't theirs, return not found
            // otherwise, if the category exists and is theirs, delete
            if (category != null && category.AppUserId != userId) return NotFound();
            else if (category != null) await _categoryService.DeleteCategoryAsync(category);

            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/EmailCategory/5
        public async Task<IActionResult> EmailCategory(int? id, string? swalMessage = null)
        {
            // Pass along Sweet Alert message if there is one
            ViewData["SwalMessage"] = swalMessage;

            if (id is null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            Category? category = await _categoryService.GetUserCategoryByIdAsync(id, userId);
            if (category == null) return NotFound();

            // Build list of contacts in that category
            List<Contact> emailContacts = new();
            emailContacts = category.Contacts.ToList();

            // build string containing the emails for each of those contacts
            StringBuilder stringBuilder = new();
            foreach (var contact in emailContacts)
            {
                stringBuilder.Append($"{contact.EmailAddress};");
            }
            //// remove final ; then build string
            stringBuilder.Remove(stringBuilder.ToString().Length - 1, 1);
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
    }
}