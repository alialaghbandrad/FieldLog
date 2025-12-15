using FieldLog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FieldLog.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<DailyLog> DailyLogs => Set<DailyLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Project>()
            .HasMany(p => p.DailyLogs)
            .WithOne(l => l.Project!)
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Project>()
            .HasIndex(p => new { p.OwnerId, p.CreatedAt });

        builder.Entity<DailyLog>()
            .HasIndex(l => new { l.ProjectId, l.LogDate });
    }
}

