using System.ComponentModel.DataAnnotations;

namespace FieldLog.Models;

public class DailyLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    [Required]
    public string CreatedById { get; set; } = "";

    [Required]
    public DateOnly LogDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    // 1) Daily events
    public string EventsJson { get; set; } = "[]";

    // 2) Weather conditions
    public string WeatherJson { get; set; } = "{}";

    // 3) Subcontractor progress
    public string SubcontractorsJson { get; set; } = "[]";

    // 4) Photos
    public string PhotoUrlsJson { get; set; } = "[]";

    // 5) Issues / insights
    public string IssuesJson { get; set; } = "[]";

    // 6) Safety incidents / near-misses
    public string SafetyJson { get; set; } = "[]";

    // 7) Labor & equipment utilization
    public string LaborJson { get; set; } = "[]";
    public string EquipmentJson { get; set; } = "[]";

    // 8) Deliveries & inspections
    public string DeliveriesJson { get; set; } = "[]";
    public string InspectionsJson { get; set; } = "[]";

    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
