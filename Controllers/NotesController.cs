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
using Rolodex.Services.Interfaces;

namespace Rolodex.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly ICategoryService _categoryService;

        public NotesController(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            IImageService imageService,
            ICategoryService categoryService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _categoryService = categoryService;
        }

        // GET: Notes
        public async Task<IActionResult> Index()
        {
            // get the user's notes
            string userId = _userManager.GetUserId(User)!;
            List<Note> notes = await _context.Notes
                .Where(c => c.AppUserId == userId)
                .ToListAsync();

            // order by date, newest to oldest
            notes.OrderBy(d => d.CreatedDate).ToList();

            return View(notes);
        }

        // GET: Notes/Create
        public async Task<IActionResult> Create()
        {
            string userId = _userManager.GetUserId(User)!;

            List<Category> categories = await _context.Categories
                .Where(c => c.AppUserId == userId).ToListAsync();
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name");

            return View();
        }

        // POST: Notes/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NoteTitle,CreatedDate,NoteText,AudioData,AudioType,ImageFile")] Note note, List<int> selected)
        {
            ModelState.Remove("AppUserId");
            if (ModelState.IsValid)
            {
                // Set User Id
                note.AppUserId = _userManager.GetUserId(User);

                // Set Created Date
                note.CreatedDate = DateTime.Now;

                // Set Image data if one has been chosen
                if (note.ImageFile != null)
                {
                    // Convert file to byte array and assign it to image data
                    note.ImageData = await _imageService.ConvertFileToByteArrayAsync(note.ImageFile);
                    // Assign ImageType based on the chosen file
                    note.ImageType = note.ImageFile.ContentType;
                }

                // handle categories

                // Handle Audio

                _context.Add(note);
                await _context.SaveChangesAsync();
                await _categoryService.AddCategoriesToNoteAsync(selected, note.Id);
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        // GET: Notes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            Note? note = await _context.Notes
                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == userId);
            if (note == null) return NotFound();

            List<Category> categories = await _context.Categories
                .Where(c => c.AppUserId == userId).ToListAsync();
            ViewData["CategoryList"] = new MultiSelectList(categories, "Id", "Name");

            return View(note);
        }

        // POST: Notes/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,NoteTitle,CreatedDate,NoteText,AudioData,AudioType")] Note note)
        {
            if (id != note.Id) return NotFound();

            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                try
                {
                    // Set User Id
                    note.AppUserId = _userManager.GetUserId(User);

                    // Set Image data if one has been chosen
                    if (note.ImageFile != null)
                    {
                        // Convert file to byte array and assign it to image data
                        note.ImageData = await _imageService.ConvertFileToByteArrayAsync(note.ImageFile);
                        // Assign ImageType based on the chosen file
                        note.ImageType = note.ImageFile.ContentType;
                    }

                    // Handle Audio

                    _context.Update(note);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteExists(note.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        // GET: Notes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            string userId = _userManager.GetUserId(User)!;

            // Get the first Note with Id matching id that belongs to the logged in user
            // else return default
            Note? note = await _context.Notes
                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == userId);

            if (note == null) return NotFound();

            return View(note);
        }

        // POST: Notes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string userId = _userManager.GetUserId(User)!;

            if (_context.Notes == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Note' is null.");
            }
            Note? note = await _context.Notes.FindAsync(id);

            // if user is attempting to delete a note that isn't theirs, return not found
            // otherwise, if the note exists and is theirs, delete
            if (note != null && note.AppUserId != userId) return NotFound();
            else if (note != null) _context.Notes.Remove(note);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NoteExists(int id)
        {
            return (_context.Notes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
