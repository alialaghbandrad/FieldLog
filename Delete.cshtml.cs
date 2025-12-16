using FieldLog.Data;
using FieldLog.Models;
using FieldLog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FieldLog.Pages.Logs;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public DeleteModel(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = "";
    public string LogDate { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        var log = await Authz.GetOwnedLogAsync(_db, userId, Id);
        if (log == null) return NotFound();

        ProjectId = log.ProjectId;
        ProjectName = log.Project?.Name ?? "";
        LogDate = log.LogDate.ToString("yyyy-MM-dd");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        var log = await Authz.GetOwnedLogAsync(_db, userId, Id);
        if (log == null) return NotFound();

        var projectId = log.ProjectId;

        _db.DailyLogs.Remove(log);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Projects/Details", new { id = projectId });
    }
}
