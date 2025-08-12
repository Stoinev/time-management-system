using Identity.Data;
using Identity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Identity.ViewModels;

namespace Identity.Controllers
{
    public class TagController : Controller
    {
        private readonly IdentityDbContext _context;

        public TagController(IdentityDbContext context)
        {
            _context = context;
        }

        // GET: Tag
        public async Task<IActionResult> Index()
        {
            var tags = await _context.Tags
                .Select(t => new TagViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    ColorCode = t.ColorCode,
                    CreatedDate = t.CreatedDate,
                    TaskCount = t.TaskTags.Count
                })
                .OrderBy(t => t.Name)
                .ToListAsync();

            return View(tags);
        }

        // GET: Tag/Create
        public IActionResult Create()
        {
            return View(new Tag { ColorCode = "#0066CC" }); // Default blue color
        }

        // POST: Tag/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tag tag)
        {
            if (await _context.Tags.AnyAsync(t => t.Name == tag.Name))
            {
                ModelState.AddModelError("Name", "A tag with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                tag.CreatedDate = DateTime.Now;
                _context.Add(tag);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tag created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(tag);
        }

        // GET: Tag/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            return View(tag);
        }

        // POST: Tag/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tag tag)
        {
            if (id != tag.Id)
            {
                return NotFound();
            }

            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name == tag.Name && t.Id != id);

            if (existingTag != null)
            {
                ModelState.AddModelError("Name", "A tag with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalTag = await _context.Tags.FindAsync(id);
                    originalTag.Name = tag.Name;
                    originalTag.ColorCode = tag.ColorCode;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Tag updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(tag.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(tag);
        }

        // GET: Tag/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tag = await _context.Tags
                .Include(t => t.TaskTags)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tag == null)
            {
                return NotFound();
            }

            ViewBag.TaskCount = tag.TaskTags.Count;
            return View(tag);
        }

        // POST: Tag/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Tag deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}