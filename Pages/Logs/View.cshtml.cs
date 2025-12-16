using System.Text.Json;
using FieldLog.Data;
using FieldLog.Models;
using FieldLog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FieldLog.Pages.Logs;

[Authorize]
public class ViewModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public ViewModel(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public Guid LogId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = "";
    public string LogDate { get; set; } = "";

    public string WeatherBadge { get; set; } = "—";
    public WeatherDto Weather { get; set; } = new();

    public List<EventDto> Events { get; set; } = new();
    public List<SubDto> Subs { get; set; } = new();
    public List<IssueDto> Issues { get; set; } = new();
    public List<SafetyDto> Safety { get; set; } = new();
    public List<LaborDto> Labor { get; set; } = new();
    public List<EquipDto> Equipment { get; set; } = new();
    public List<DeliveryDto> Deliveries { get; set; } = new();
    public List<InspectDto> Inspections { get; set; } = new();

    public List<string> PhotoUrls { get; set; } = new();
    public string? Notes { get; set; }

    // Must match JSON keys used in New/Edit pages
    public record EventDto(string? time, string? title, string? details);
    public record SubDto(string? company, string? trade, int workers, string? workDone, int percent);
    public record IssueDto(string? type, string? severity, string? description, string? status);
    public record SafetyDto(string? category, string? description, string? reportedTo);
    public record LaborDto(string? trade, int workers, double hours, string? notes);
    public record EquipDto(string? name, double hours, string? condition, string? notes);
    public record DeliveryDto(string? vendor, string? material, string? qty, string? time);
    public record InspectDto(string? type, string? inspector, string? result);
    public record WeatherDto(double? tempC = null, double? windKph = null, string? precip = null, string? notes = null);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userId = _userManager.GetUserId(User)!;

        var log = await Authz.GetOwnedLogAsync(_db, userId, id);
        if (log == null) return NotFound();

        LogId = log.Id;
        ProjectId = log.ProjectId;
        ProjectName = log.Project?.Name ?? "";
        LogDate = log.LogDate.ToString("yyyy-MM-dd");
        Notes = log.Notes;

        Weather = SafeObj<WeatherDto>(log.WeatherJson) ?? new WeatherDto();
        WeatherBadge = JsonHelper.SummaryWeather(log.WeatherJson);

        Events = SafeList<EventDto>(log.EventsJson);
        Subs = SafeList<SubDto>(log.SubcontractorsJson);
        Issues = SafeList<IssueDto>(log.IssuesJson);
        Safety = SafeList<SafetyDto>(log.SafetyJson);
        Labor = SafeList<LaborDto>(log.LaborJson);
        Equipment = SafeList<EquipDto>(log.EquipmentJson);
        Deliveries = SafeList<DeliveryDto>(log.DeliveriesJson);
        Inspections = SafeList<InspectDto>(log.InspectionsJson);

        PhotoUrls = SafeList<string>(log.PhotoUrlsJson);

        return Page();
    }

    private static List<T> SafeList<T>(string json)
    {
        try { return JsonSerializer.Deserialize<List<T>>(json, JsonOpts) ?? new List<T>(); }
        catch { return new List<T>(); }
    }

    private static T? SafeObj<T>(string json)
    {
        try { return JsonSerializer.Deserialize<T>(json, JsonOpts); }
        catch { return default; }
    }
}
