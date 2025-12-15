using FieldLog.Data;
using FieldLog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FieldLog.Services;

public static class Authz
{
    public static async Task<Project?> GetOwnedProjectAsync(
        AppDbContext db, string userId, Guid projectId)
    {
        return await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);
    }

    public static async Task<Project?> GetOwnedProjectWithLogsAsync(
        AppDbContext db, string userId, Guid projectId)
    {
        return await db.Projects
            .Include(p => p.DailyLogs)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.OwnerId == userId);
    }

    public static async Task<DailyLog?> GetOwnedLogAsync(
        AppDbContext db, string userId, Guid logId)
    {
        return await db.DailyLogs
            .Include(l => l.Project)
            .FirstOrDefaultAsync(l =>
                l.Id == logId &&
                l.Project != null &&
                l.Project.OwnerId == userId);
    }
}
