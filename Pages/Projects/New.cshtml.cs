using System.ComponentModel.DataAnnotations;
using FieldLog.Data;
using FieldLog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FieldLog.Pages.Projects;

[Authorize]
public class NewModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public NewModel(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty, Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [BindProperty, MaxLength(300)]
    public string? Location { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var userId = _userManager.GetUserId(User)!;
        var project = new Project
        {
            Name = Name.Trim(),
            Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim(),
            OwnerId = userId
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Projects/Details", new { id = project.Id });
    }
}
