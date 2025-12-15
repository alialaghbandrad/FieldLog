using System.Reflection;
using FieldLog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FieldLog.Pages;

[Authorize]
public class DiagnosticsModel : PageModel
{
    private readonly IWebHostEnvironment _env;

    public DiagnosticsModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    public string EnvironmentName { get; set; } = "";
    public string AppVersion { get; set; } = "";
    public List<string> LogTailLines { get; set; } = new();

    public void OnGet()
    {
        EnvironmentName = _env.EnvironmentName;
        AppVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

        // Serilog writes to "logs/fieldlog-YYYYMMDD.log"
        var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
        // In dev, AppContext.BaseDirectory points to bin; we also try project root logs
        var altLogDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");

        var file = FindLatestLogFile(logDir) ?? FindLatestLogFile(altLogDir);
        if (file == null) return;

        LogTailLines = Tail(file, 200);
    }

    private static string? FindLatestLogFile(string dir)
    {
        if (!Directory.Exists(dir)) return null;

        var files = Directory.GetFiles(dir, "fieldlog-*.log")
            .Select(f => new FileInfo(f))
            .OrderByDescending(fi => fi.LastWriteTimeUtc)
            .ToList();

        return files.FirstOrDefault()?.FullName;
    }

    private static List<string> Tail(string filePath, int lines)
    {
        try
        {
            var all = System.IO.File.ReadAllLines(filePath);
            return all.Length <= lines ? all.ToList() : all.Skip(all.Length - lines).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }
}
