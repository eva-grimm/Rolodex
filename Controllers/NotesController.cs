using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rolodex.Data;
using Rolodex.Models;
using Rolodex.Services;
using Rolodex.Services.Interfaces;

namespace Rolodex.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly ICategoryService _categoryService;
        private readonly INoteService _noteService;

        public NotesController(UserManager<AppUser> userManager,
            IImageService imageService,
            ICategoryService categoryService,
            INoteService noteService)
        {
            _userManager = userManager;
            _imageService = imageService;
            _categoryService = categoryService;
            _noteService = noteService;
        }

        // GET: Notes
        public async Task<IActionResult> Index(int? categoryId)
        {
            // get the user's notes
            string userId = _userManager.GetUserId(User)!;
            List<Note> notes = (await _noteService.GetUserNotesAsync(userId)).ToList();

            // and user's categories
            IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);

            // what will contain the filter result
            List<Note> model = new();

            // filter based on category
            if (categoryId is null) model = notes;
            else model = categories.SelectMany(c => c.Notes).ToList();

            // order
            model.OrderBy(n => n.Updated).ThenBy(n => n.Created);

            // provide the user's categories
            ViewData["CategoriesList"] = new SelectList(categories, "Id", "Name", categoryId);

            return View(model);
        }

        // GET: Filtered Contacts
        public async Task<IActionResult> SearchNotes(string? searchString)
        {
            // get the user's contacts
            string userId = _userManager.GetUserId(User)!;
            List<Note> notes = (await _noteService.GetUserNotesAsync(userId))
                .ToList();

            // what will contain the search result
            List<Note> model = new();

            // populate model with results
            if (string.IsNullOrEmpty(searchString)) model = notes;
            else model = notes.Where(c => c.NoteTitle!.ToLower().Contains(searchString.ToLower())
                || c.NoteText?.ToLower().Contains(searchString.ToLower()) == true)
                .OrderBy(n => n.Updated)
                    .ThenBy(n => n.Created)
                .ToList();

            // provide the user's categories as well
            IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
            ViewData["CategoriesList"] = new MultiSelectList(categories, "Id", "Name");

            // put the user's search into the ViewBag
            ViewData["SearchString"] = searchString;

            return View(nameof(Index), model);
        }

        // GET: Notes/Create
        public async Task<IActionResult> Create()
        {
            string userId = _userManager.GetUserId(User)!;

            IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name");

            return View();
        }

        // POST: Notes/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NoteTitle,Created,NoteText,AudioData,AudioType,ImageFile")] Note note, List<int> selected)
        {
            ModelState.Remove("AppUserId");

            if (!ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User)!;

                IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
                ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", selected);

                return View(note);
            }

            try
            {
                note.AppUserId = _userManager.GetUserId(User);
                note.Created = DateTime.Now;

                // Set Image data if one has been chosen
                if (note.ImageFile != null)
                {
                    // Convert file to byte array and assign it to image data
                    note.ImageData = await _imageService.ConvertFileToByteArrayAsync(note.ImageFile);
                    // Assign ImageType based on the chosen file
                    note.ImageType = note.ImageFile.ContentType;
                }

                await _noteService.AddNoteAsync(note);
                await _categoryService.AddCategoriesToNoteAsync(selected, note.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                string userId = _userManager.GetUserId(User)!;

                IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
                ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", selected);

                return View(note);
            }
        }

        // GET: Notes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            Note? note = await _noteService.GetNoteByIdAsync(id, userId);
            if (note == null) return NotFound();

            IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
            List<int> categoryIds = note.Categories.Select(n => n.Id).ToList();
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", categoryIds);

            return View(note);
        }

        // POST: Notes/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,NoteTitle,Created,Updated,NoteText,AudioData,AudioType,ImageData,ImageType,ImageFile")] Note note, List<int> selected)
        {
            if (id != note.Id) return NotFound();
            string userId = _userManager.GetUserId(User)!;

            ModelState.Remove("AppUserId");

            if (!ModelState.IsValid)
            {
                IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
                List<int> categoryIds = note.Categories.Select(n => n.Id).ToList();
                ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", categoryIds);

                return View(note);
            }

            try
            {
                note.AppUserId = _userManager.GetUserId(User);
                note.Updated = DateTime.Now;

                // Set Image data if one has been chosen
                if (note.ImageFile != null)
                {
                    // Convert file to byte array and assign it to image data
                    note.ImageData = await _imageService.ConvertFileToByteArrayAsync(note.ImageFile);
                    // Assign ImageType based on the chosen file
                    note.ImageType = note.ImageFile.ContentType;
                }

                await _noteService.UpdateNoteAsync(note);
                await _categoryService.AddCategoriesToNoteAsync(selected, note.Id);

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_noteService.NoteExists(note.Id)) return NotFound();
                else
                {

                    IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
                    List<int> categoryIds = note.Categories.Select(n => n.Id).ToList();
                    ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", categoryIds);

                    return View(note);
                }
            }
            catch (Exception)
            {
                IEnumerable<Category> categories = await _categoryService.GetUserCategoriesAsync(userId);
                List<int> categoryIds = note.Categories.Select(n => n.Id).ToList();
                ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name", categoryIds);

                return View(note);
            }
        }

        // GET: Notes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            Note? note = await _noteService.GetNoteByIdAsync(id, userId);
            if (note == null) return NotFound();

            return View(note);
        }

        // POST: Notes/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string userId = _userManager.GetUserId(User)!;

            Note? note = await _noteService.GetNoteByIdAsync(id, userId);

            // if user is attempting to delete a note that isn't theirs, return not found
            // otherwise, if the note exists and is theirs, delete
            if (note != null && note.AppUserId != userId) return NotFound();
            else if (note != null) await _noteService.DeleteNoteAsync(note);

            return RedirectToAction(nameof(Index));
        }
    }
}