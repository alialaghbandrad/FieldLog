using System.Text.Json;

namespace FieldLog.Services;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public static string NormalizeOrThrow(string json, bool expectArray, string fieldName)
    {
        json = (json ?? "").Trim();
        if (string.IsNullOrWhiteSpace(json))
            return expectArray ? "[]" : "{}";

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (expectArray && root.ValueKind != JsonValueKind.Array)
                throw new InvalidOperationException($"{fieldName} must be a JSON array (e.g., []).");

            if (!expectArray && root.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException($"{fieldName} must be a JSON object (e.g., {{}}).");

            // Re-serialize normalized
            return JsonSerializer.Serialize(root, Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"{fieldName} has invalid JSON: {ex.Message}");
        }
    }

    public static int CountArrayItems(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return 0;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return 0;
            return doc.RootElement.GetArrayLength();
        }
        catch
        {
            return 0;
        }
    }

    public static string SummaryWeather(string weatherJson)
    {
        // Very lightweight summary: tries to read common keys
        if (string.IsNullOrWhiteSpace(weatherJson)) return "—";
        try
        {
            using var doc = JsonDocument.Parse(weatherJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return "—";

            string temp = TryGet(doc, "tempC");
            string wind = TryGet(doc, "windKph");
            string precip = TryGet(doc, "precip");
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(temp)) parts.Add($"{temp}°C");
            if (!string.IsNullOrWhiteSpace(wind)) parts.Add($"Wind {wind} kph");
            if (!string.IsNullOrWhiteSpace(precip)) parts.Add($"Precip {precip}");

            return parts.Count == 0 ? "—" : string.Join(", ", parts);
        }
        catch
        {
            return "—";
        }

        static string TryGet(JsonDocument doc, string key)
        {
            if (doc.RootElement.TryGetProperty(key, out var p))
            {
                return p.ValueKind switch
                {
                    JsonValueKind.Number => p.GetRawText(),
                    JsonValueKind.String => p.GetString() ?? "",
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => ""
                };
            }
            return "";
        }
    }
}
