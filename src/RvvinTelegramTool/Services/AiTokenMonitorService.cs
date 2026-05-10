using System.Globalization;
using System.Text.Json;
using RvvinTelegramTool.Models;

namespace RvvinTelegramTool.Services;

public sealed class AiTokenMonitorService
{
    private readonly AppFolders _folders;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public AiTokenMonitorService(AppFolders folders)
    {
        _folders = folders;
    }

    private string UsageStorePath => Path.Combine(_folders.Settings, "ai-token-usage.json");

    private string MonitorSettingsPath => Path.Combine(_folders.Settings, "ai-token-monitor-settings.json");

    public IReadOnlyList<AiTokenUsageRecord> LoadUsage()
    {
        if (!File.Exists(UsageStorePath))
        {
            return Array.Empty<AiTokenUsageRecord>();
        }

        var json = File.ReadAllText(UsageStorePath);
        return JsonSerializer.Deserialize<List<AiTokenUsageRecord>>(json) ?? Array.Empty<AiTokenUsageRecord>();
    }

    public void SaveUsage(IEnumerable<AiTokenUsageRecord> records)
    {
        var ordered = records.OrderByDescending(record => record.UsedAt).ToList();
        File.WriteAllText(UsageStorePath, JsonSerializer.Serialize(ordered, _jsonOptions));
    }

    public AiTokenMonitorSettings LoadSettings()
    {
        if (!File.Exists(MonitorSettingsPath))
        {
            return new AiTokenMonitorSettings();
        }

        var json = File.ReadAllText(MonitorSettingsPath);
        return JsonSerializer.Deserialize<AiTokenMonitorSettings>(json) ?? new AiTokenMonitorSettings();
    }

    public void SaveSettings(AiTokenMonitorSettings settings)
    {
        File.WriteAllText(MonitorSettingsPath, JsonSerializer.Serialize(settings, _jsonOptions));
    }

    public AiTokenMonitorSummary BuildSummary(IEnumerable<AiTokenUsageRecord> records, int tokenQuota, int alertThreshold)
    {
        var now = DateTime.Now;
        var list = records.ToList();
        var usedTokens = list.Sum(record => record.TotalTokens);
        var remainingTokens = Math.Max(0, tokenQuota - usedTokens);
        var todayTokens = list.Where(record => record.UsedAt.Date == now.Date).Sum(record => record.TotalTokens);
        var monthTokens = list.Where(record => record.UsedAt.Year == now.Year && record.UsedAt.Month == now.Month).Sum(record => record.TotalTokens);
        var usedCost = list.Sum(record => record.CostUsd);
        var status = remainingTokens <= alertThreshold
            ? "余额低于预警阈值，请及时充值或降低高消耗任务。"
            : "余额正常，持续监控过往用量趋势。";

        return new AiTokenMonitorSummary(tokenQuota, usedTokens, remainingTokens, usedCost, todayTokens, monthTokens, alertThreshold, status);
    }

    public IReadOnlyList<AiTokenUsageRecord> ImportCsv(string path)
    {
        return File.ReadLines(path)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Where(line => !line.TrimStart().StartsWith('#'))
            .SkipWhile(line => LooksLikeHeader(line))
            .Select(ParseLine)
            .Where(record => record is not null)
            .Cast<AiTokenUsageRecord>()
            .ToList();
    }

    public string ExportCsv(IEnumerable<AiTokenUsageRecord> records)
    {
        var path = Path.Combine(_folders.Exports, $"ai_token_usage_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var lines = new List<string>
        {
            "UsedAt,Provider,Model,Scenario,PromptTokens,CompletionTokens,CachedTokens,TotalTokens,CostUsd,Note"
        };
        lines.AddRange(records.OrderByDescending(record => record.UsedAt).Select(record => string.Join(',', new[]
        {
            Escape(record.UsedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
            Escape(record.Provider),
            Escape(record.Model),
            Escape(record.Scenario),
            record.PromptTokens.ToString(CultureInfo.InvariantCulture),
            record.CompletionTokens.ToString(CultureInfo.InvariantCulture),
            record.CachedTokens.ToString(CultureInfo.InvariantCulture),
            record.TotalTokens.ToString(CultureInfo.InvariantCulture),
            record.CostUsd.ToString("0.####", CultureInfo.InvariantCulture),
            Escape(record.Note)
        })));
        File.WriteAllLines(path, lines);
        return path;
    }

    public AiTokenUsageRecord CreateManualRecord(string provider, string model, string scenario, int promptTokens, int completionTokens, int cachedTokens, decimal costUsd, string note)
    {
        return new AiTokenUsageRecord
        {
            UsedAt = DateTime.Now,
            Provider = string.IsNullOrWhiteSpace(provider) ? "OpenAI" : provider.Trim(),
            Model = string.IsNullOrWhiteSpace(model) ? "unknown" : model.Trim(),
            Scenario = string.IsNullOrWhiteSpace(scenario) ? "手动记录" : scenario.Trim(),
            PromptTokens = Math.Max(0, promptTokens),
            CompletionTokens = Math.Max(0, completionTokens),
            CachedTokens = Math.Max(0, cachedTokens),
            CostUsd = Math.Max(0, costUsd),
            Note = note.Trim()
        };
    }

    private static AiTokenUsageRecord? ParseLine(string line)
    {
        var columns = SplitCsv(line);
        if (columns.Count < 7)
        {
            return null;
        }

        return new AiTokenUsageRecord
        {
            UsedAt = ParseDate(columns.ElementAtOrDefault(0)),
            Provider = DefaultText(columns.ElementAtOrDefault(1), "OpenAI"),
            Model = DefaultText(columns.ElementAtOrDefault(2), "unknown"),
            Scenario = DefaultText(columns.ElementAtOrDefault(3), "导入记录"),
            PromptTokens = ParseInt(columns.ElementAtOrDefault(4)),
            CompletionTokens = ParseInt(columns.ElementAtOrDefault(5)),
            CachedTokens = ParseInt(columns.ElementAtOrDefault(6)),
            CostUsd = ParseDecimal(columns.ElementAtOrDefault(7)),
            Note = columns.ElementAtOrDefault(8) ?? string.Empty
        };
    }

    private static bool LooksLikeHeader(string line)
    {
        var first = SplitCsv(line).FirstOrDefault() ?? string.Empty;
        return first.Contains("date", StringComparison.OrdinalIgnoreCase)
            || first.Contains("time", StringComparison.OrdinalIgnoreCase)
            || first.Contains("日期", StringComparison.OrdinalIgnoreCase)
            || first.Contains("时间", StringComparison.OrdinalIgnoreCase);
    }

    private static DateTime ParseDate(string? value)
    {
        return DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var result)
            ? result
            : DateTime.Now;
    }

    private static int ParseInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? Math.Max(0, result) : 0;
    }

    private static decimal ParseDecimal(string? value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) ? Math.Max(0, result) : 0;
    }

    private static string DefaultText(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static List<string> SplitCsv(string line)
    {
        var result = new List<string>();
        var current = new List<char>();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Add('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if ((c == ',' || c == '\t') && !inQuotes)
            {
                result.Add(new string(current.ToArray()).Trim());
                current.Clear();
            }
            else
            {
                current.Add(c);
            }
        }

        result.Add(new string(current.ToArray()).Trim());
        return result;
    }

    private static string Escape(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
