using System.Text;
using FieldLog.Data;
using FieldLog.Models;
using FieldLog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FieldLog.Pages;

[Authorize]
public class ExportModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public ExportModel(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty]
    public Guid ProjectId { get; set; }

    public void OnGet(Guid? projectId)
    {
        if (projectId.HasValue) ProjectId = projectId.Value;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = _userManager.GetUserId(User)!;

        var project = await _db.Projects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == ProjectId && p.OwnerId == userId);

        if (project == null) return NotFound();

        var logs = await _db.DailyLogs.AsNoTracking()
            .Where(l => l.ProjectId == ProjectId)
            .OrderBy(l => l.LogDate)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Project,Date,Weather,EventsCount,SubcontractorsCount,IssuesCount,SafetyCount,PhotosCount,LaborCount,EquipmentCount,DeliveriesCount,InspectionsCount,Notes");

        foreach (var l in logs)
        {
            var weather = JsonHelper.SummaryWeather(l.WeatherJson).Replace(",", ";");
            var notes = (l.Notes ?? "").Replace("\"", "\"\"");

            sb.Append('"').Append(project.Name.Replace("\"", "\"\"")).Append('"').Append(',');
            sb.Append(l.LogDate.ToString("yyyy-MM-dd")).Append(',');
            sb.Append('"').Append(weather).Append('"').Append(',');
            sb.Append(JsonHelper.CountArrayItems(l.EventsJson)).Append(',');
            sb.Append(JsonHelper.CountArrayItems(l.SubcontractorsJson)).Append(',');
            sb.Append(JsonHelper.CountArrayItems(l.IssuesJson)).Append(',');
            sb.Append(JsonHelper.CountArrayItems(l.SafetyJson)).Append(',');
            sb.Append(JsonHelper.CountArrayItems(l.PhotoUrlsJson)).Append(',');
            sb.Append(JsonHelper.CountArrayItems(l.LaborJson)).Append(',');
            sb.Append(JsonHelper.CountArrayItems(l.EquipmentJson)).Append(',');
            sb.Append(JsonHelper.CountArrayItems(l.DeliveriesJson)).Append(',');
            sb.Append(JsonHelper.CountArrayItems(l.InspectionsJson)).Append(',');
            sb.Append('"').Append(notes).Append('"').AppendLine();
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"FieldLog_{project.Name}_{DateTime.UtcNow:yyyyMMddHHmm}.csv".Replace(" ", "_");
        return File(bytes, "text/csv", fileName);
    }
}
