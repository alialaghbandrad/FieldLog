using FieldLog.Data;
using FieldLog.Models;
using FieldLog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FieldLog.Pages.Projects;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public DetailsModel(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = "";
    public string? ProjectLocation { get; set; }

    public string? FromStr { get; set; }
    public string? ToStr { get; set; }

    public record LogRow(
        Guid LogId,
        string LogDate,
        string WeatherSummary,
        int SubcontractorCount,
        int OpenIssuesCount,
        int SafetyCount,
        int PhotoCount
    );

    public List<LogRow> Logs { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, DateTime? from, DateTime? to)
    {
        var userId = _userManager.GetUserId(User)!;

        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (project == null)
            return NotFound();

        ProjectId = project.Id;
        ProjectName = project.Name;
        ProjectLocation = project.Location;

        FromStr = from?.ToString("yyyy-MM-dd");
        ToStr = to?.ToString("yyyy-MM-dd");

        var query = _db.DailyLogs
            .AsNoTracking()
            .Where(l => l.ProjectId == id);

        if (from.HasValue)
        {
            var d = DateOnly.FromDateTime(from.Value);
            query = query.Where(l => l.LogDate >= d);
        }

        if (to.HasValue)
        {
            var d = DateOnly.FromDateTime(to.Value);
            query = query.Where(l => l.LogDate <= d);
        }

        var logs = await query
            .OrderByDescending(l => l.LogDate)
            .ToListAsync();

        Logs = logs.Select(l =>
        {
            var issuesTotal = JsonHelper.CountArrayItems(l.IssuesJson);

            return new LogRow(
                l.Id,
                l.LogDate.ToString("yyyy-MM-dd"),
                JsonHelper.SummaryWeather(l.WeatherJson),
                JsonHelper.CountArrayItems(l.SubcontractorsJson),
                issuesTotal,
                JsonHelper.CountArrayItems(l.SafetyJson),
                JsonHelper.CountArrayItems(l.PhotoUrlsJson)
            );
        }).ToList();

        return Page();
    }
}
