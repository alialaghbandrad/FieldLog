using FieldLog.Data;
using FieldLog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FieldLog.Pages.Projects
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly AppDbContext _context;

        public DeleteModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Project Project { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            Project = project;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var project = await _context.Projects.FindAsync(Project.Id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Projects/Index");
        }
    }
}

