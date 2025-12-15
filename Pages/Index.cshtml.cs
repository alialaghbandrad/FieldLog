using FieldLog.Data;
using FieldLog.Models;
using FieldLog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FieldLog.Pages;

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

    public int ProjectCount { get; set; }
    public int LogCount { get; set; }
    public int PhotoCount { get; set; }
    public List<Project> RecentProjects { get; set; } = new();

    public async Task OnGet()
    {
        var userId = _userManager.GetUserId(User)!;

        ProjectCount = await _db.Projects.AsNoTracking()
            .CountAsync(p => p.OwnerId == userId);

        var projectIds = await _db.Projects.AsNoTracking()
            .Where(p => p.OwnerId == userId)
            .Select(p => p.Id)
            .ToListAsync();

        LogCount = await _db.DailyLogs.AsNoTracking()
            .CountAsync(l => projectIds.Contains(l.ProjectId));

        // Count photos by counting items in PhotoUrlsJson arrays (approx)
        var photoJsons = await _db.DailyLogs.AsNoTracking()
            .Where(l => projectIds.Contains(l.ProjectId))
            .Select(l => l.PhotoUrlsJson)
            .ToListAsync();

        PhotoCount = photoJsons.Sum(j => JsonHelper.CountArrayItems(j));

        RecentProjects = await _db.Projects.AsNoTracking()
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();
    }
}

