using FieldLog.Data;
using FieldLog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FieldLog.Pages.Projects;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public IndexModel(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<Project> Projects { get; set; } = new();

    public async Task OnGet()
    {
        var userId = _userManager.GetUserId(User)!;
        Projects = await _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
