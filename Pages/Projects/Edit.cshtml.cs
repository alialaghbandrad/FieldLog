using System.ComponentModel.DataAnnotations;
using FieldLog.Data;
using FieldLog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FieldLog.Pages.Projects;

[Authorize]
public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public EditModel(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Project Name is required.")]
    [StringLength(200, ErrorMessage = "Project Name is too long (max 200 characters).")]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    [StringLength(200, ErrorMessage = "Location is too long (max 200 characters).")]
    public string? Location { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userId = _userManager.GetUserId(User)!;

        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (project == null)
            return NotFound();

        Id = project.Id;
        Name = project.Name;
        Location = project.Location;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid)
            return Page();

        var userId = _userManager.GetUserId(User)!;

        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (project == null)
            return NotFound();

        project.Name = Name.Trim();
        project.Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim();

        await _db.SaveChangesAsync();

        return RedirectToPage("/Projects/Details", new { id = project.Id });
    }
}
