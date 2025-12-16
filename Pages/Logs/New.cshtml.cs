using System.ComponentModel.DataAnnotations;
using FieldLog.Data;
using FieldLog.Models;
using FieldLog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FieldLog.Pages.Logs;

[Authorize]
[RequestSizeLimit(50_000_000)] // ~50MB
public class NewModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public NewModel(AppDbContext db, UserManager<AppUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [BindProperty]
    public Guid ProjectId { get; set; }

    public string ProjectName { get; set; } = "";

    [BindProperty, Required]
    public DateTime LogDate { get; set; } = DateTime.Today;

    // JSON fields
    [BindProperty] public string EventsJson { get; set; } = "[]";
    [BindProperty] public string WeatherJson { get; set; } = "{}";
    [BindProperty] public string SubcontractorsJson { get; set; } = "[]";
    [BindProperty] public string IssuesJson { get; set; } = "[]";
    [BindProperty] public string SafetyJson { get; set; } = "[]";
    [BindProperty] public string LaborJson { get; set; } = "[]";
    [BindProperty] public string EquipmentJson { get; set; } = "[]";
    [BindProperty] public string DeliveriesJson { get; set; } = "[]";
    [BindProperty] public string InspectionsJson { get; set; } = "[]";

    [BindProperty] public string? Notes { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid projectId)
    {
        var userId = _userManager.GetUserId(User)!;

        var project = await _db.Projects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);

        if (project == null) return NotFound();

        ProjectId = project.Id;
        ProjectName = project.Name;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(List<IFormFile> Photos)
    {
        var userId = _userManager.GetUserId(User)!;

        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == ProjectId && p.OwnerId == userId);

        if (project == null) return NotFound();

        ProjectName = project.Name;

        if (!ModelState.IsValid) return Page();

        // Validate JSON early
        try
        {
            EventsJson = JsonHelper.NormalizeOrThrow(EventsJson, expectArray: true, fieldName: "EventsJson");
            WeatherJson = JsonHelper.NormalizeOrThrow(WeatherJson, expectArray: false, fieldName: "WeatherJson");
            SubcontractorsJson = JsonHelper.NormalizeOrThrow(SubcontractorsJson, true, "SubcontractorsJson");
            IssuesJson = JsonHelper.NormalizeOrThrow(IssuesJson, true, "IssuesJson");
            SafetyJson = JsonHelper.NormalizeOrThrow(SafetyJson, true, "SafetyJson");
            LaborJson = JsonHelper.NormalizeOrThrow(LaborJson, true, "LaborJson");
            EquipmentJson = JsonHelper.NormalizeOrThrow(EquipmentJson, true, "EquipmentJson");
            DeliveriesJson = JsonHelper.NormalizeOrThrow(DeliveriesJson, true, "DeliveriesJson");
            InspectionsJson = JsonHelper.NormalizeOrThrow(InspectionsJson, true, "InspectionsJson");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        // Prevent duplicate log date for same project (optional but realistic)
        var dateOnly = DateOnly.FromDateTime(LogDate);
        var existing = await _db.DailyLogs.AsNoTracking()
            .AnyAsync(l => l.ProjectId == ProjectId && l.LogDate == dateOnly);

        if (existing)
        {
            ModelState.AddModelError(string.Empty, $"A daily log already exists for {dateOnly:yyyy-MM-dd}.");
            return Page();
        }

        var log = new DailyLog
        {
            ProjectId = ProjectId,
            CreatedById = userId,
            LogDate = dateOnly,
            EventsJson = EventsJson,
            WeatherJson = WeatherJson,
            SubcontractorsJson = SubcontractorsJson,
            IssuesJson = IssuesJson,
            SafetyJson = SafetyJson,
            LaborJson = LaborJson,
            EquipmentJson = EquipmentJson,
            DeliveriesJson = DeliveriesJson,
            InspectionsJson = InspectionsJson,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            PhotoUrlsJson = "[]"
        };

        _db.DailyLogs.Add(log);
        await _db.SaveChangesAsync();

        // Save uploads and attach URLs
        if (Photos != null && Photos.Count > 0)
        {
            var urls = await SavePhotosAsync(ProjectId, log.Id, Photos);
            if (urls.Count > 0)
            {
                // store as JSON array
                var json = System.Text.Json.JsonSerializer.Serialize(urls, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                log.PhotoUrlsJson = json;
                await _db.SaveChangesAsync();
            }
        }

        return RedirectToPage("/Logs/View", new { id = log.Id });
    }

    private async Task<List<string>> SavePhotosAsync(Guid projectId, Guid logId, List<IFormFile> photos)
    {
        var results = new List<string>();
        var webRoot = _env.WebRootPath;

        // Ensure wwwroot exists (it should)
        if (string.IsNullOrWhiteSpace(webRoot))
            return results;

        var folderRel = Path.Combine("uploads", projectId.ToString(), logId.ToString());
        var folderAbs = Path.Combine(webRoot, folderRel);

        Directory.CreateDirectory(folderAbs);

        foreach (var file in photos)
        {
            if (file.Length <= 0) continue;

            // Basic safety: only allow images by content type (light check)
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                continue;

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

            var safeName = $"{Guid.NewGuid():N}{ext}";
            var absPath = Path.Combine(folderAbs, safeName);

            await using var stream = System.IO.File.Create(absPath);
            await file.CopyToAsync(stream);

            // Convert to web URL
            var url = "/" + folderRel.Replace("\\", "/") + "/" + safeName;
            results.Add(url);
        }

        return results;
    }
}
