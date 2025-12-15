using System.ComponentModel.DataAnnotations;

namespace FieldLog.Models;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [MaxLength(300)]
    public string? Location { get; set; }

    [Required]
    public string OwnerId { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<DailyLog> DailyLogs { get; set; } = new();
}
