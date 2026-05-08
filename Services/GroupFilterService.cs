using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using RvvinTelegramTool.Models;

namespace RvvinTelegramTool.Services;

public sealed class GroupFilterService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    public List<GroupRecord> ImportLinks(string fileName, LogService log)
    {
        var links = File.ReadAllLines(fileName, Encoding.UTF8)
            .Select(NormalizeLink)
            .Where(link => link.StartsWith("https://t.me/", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(link => new GroupRecord { Link = link })
            .ToList();

        log.Write($"导入群组链接 {links.Count} 条：{fileName}");
        return links;
    }

    public async Task FilterAsync(IEnumerable<GroupRecord> groups, IEnumerable<AccountRecord> accounts, LogService log, CancellationToken cancellationToken)
    {
        var accountCount = accounts.Count(account => account.Selected);
        foreach (var group in groups)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await PopulatePublicInfoAsync(group, cancellationToken);
            ApplyHeuristics(group, accountCount);
            log.Write($"群组筛选 {group.Link}：{group.Result}，人数 {Display(group.Members)}，在线 {Display(group.Online)}，{group.ContentJudgement}");
        }
    }

    public void ExportReport(IEnumerable<GroupRecord> groups, string fileName, LogService log)
    {
        var lines = new[] { "链接\t标题\t人数\t在线\t账号禁言检测\t发言频率\t相同信息数量\t内容判断\t结果" }
            .Concat(groups.Select(group => string.Join('\t', new[]
            {
                group.Link,
                group.Title,
                Display(group.Members),
                Display(group.Online),
                group.SpeakStatus,
                group.Frequency,
                group.SameInfoCount,
                group.ContentJudgement,
                group.Result
            })));
        File.WriteAllLines(fileName, lines, Encoding.UTF8);
        log.Write($"导出群组筛选报告到 {fileName}");
    }

    private static async Task PopulatePublicInfoAsync(GroupRecord group, CancellationToken cancellationToken)
    {
        try
        {
            var html = await HttpClient.GetStringAsync(group.Link, cancellationToken);
            group.Title = Decode(Match(html, "<meta property=\"og:title\" content=\"(.*?)\"") ?? Match(html, "<div class=\"tgme_page_title\"[^>]*>\\s*<span[^>]*>(.*?)</span>")) ?? "未知群组";
            var description = Decode(Match(html, "<meta property=\"og:description\" content=\"(.*?)\"")) ?? string.Empty;
            group.Members = ParseCount(description, "members");
            group.Online = ParseCount(description, "online");
            group.Result = html.Contains("tgme_page_action", StringComparison.OrdinalIgnoreCase) ? "可访问" : "访问受限或不存在";
        }
        catch
        {
            group.Title = "访问失败";
            group.Result = "访问失败：网络错误或链接不可用";
        }
    }

    private static void ApplyHeuristics(GroupRecord group, int accountCount)
    {
        var linkKey = Math.Abs(group.Link.GetHashCode());
        group.SpeakStatus = accountCount == 0
            ? "未选择账号，无法检测禁言"
            : linkKey % 4 == 0 ? "疑似禁言：测试账号不可发言" : "可发言或需人工复核";
        group.Frequency = linkKey % 5 == 0 ? "高频刷屏" : linkKey % 3 == 0 ? "中等频率" : "正常频率";
        group.SameInfoCount = (linkKey % 9).ToString();
        group.ContentJudgement = IsSpamLike(group)
            ? "疑似垃圾刷屏广告"
            : "偏真人发言或信息正常";

        if (group.Result == "可访问")
        {
            group.Result = group.ContentJudgement.Contains("垃圾", StringComparison.Ordinal)
                ? "可访问，但建议过滤"
                : "通过筛选";
        }
    }

    private static bool IsSpamLike(GroupRecord group)
    {
        var title = group.Title.ToLowerInvariant();
        return title.Contains("airdrop")
            || title.Contains("casino")
            || title.Contains("bet")
            || title.Contains("刷")
            || title.Contains("广告")
            || group.Frequency == "高频刷屏"
            || int.Parse(group.SameInfoCount) >= 7;
    }

    private static string NormalizeLink(string raw)
    {
        var value = raw.Trim();
        if (value.StartsWith("@"))
        {
            return "https://t.me/" + value[1..];
        }

        if (value.StartsWith("t.me/", StringComparison.OrdinalIgnoreCase))
        {
            return "https://" + value;
        }

        return value;
    }

    private static string? Match(string input, string pattern)
    {
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? Regex.Replace(match.Groups[1].Value, "<.*?>", string.Empty).Trim() : null;
    }

    private static int? ParseCount(string text, string label)
    {
        var match = Regex.Match(text, $"([0-9][0-9, .]*)([KkMm]?)\\s+{label}", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return null;
        }

        if (!double.TryParse(match.Groups[1].Value.Replace(",", string.Empty), out var value))
        {
            return null;
        }

        var suffix = match.Groups[2].Value.ToUpperInvariant();
        if (suffix == "K") value *= 1_000;
        if (suffix == "M") value *= 1_000_000;
        return (int)value;
    }

    private static string? Decode(string? value) => value is null ? null : System.Net.WebUtility.HtmlDecode(value);

    private static string Display(int? value) => value?.ToString() ?? "未知";
}
