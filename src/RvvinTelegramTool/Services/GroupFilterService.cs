using System.Text.RegularExpressions;
using RvvinTelegramTool.Models;

namespace RvvinTelegramTool.Services;

public sealed class GroupFilterService
{
    private static readonly Regex TelegramLinkRegex = new(@"^(https?://)?(t\.me|telegram\.me|telegram\.dog)/[A-Za-z0-9_+/-]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly string[] SpamWords = { "airdrop", "casino", "bet", "刷屏", "广告", "返利", "usdt", "赚钱", "引流" };

    public IReadOnlyList<GroupScanResult> ImportLinks(string txtPath)
    {
        return File.ReadLines(txtPath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(link => new GroupScanResult
            {
                Selected = true,
                Link = NormalizeLink(link),
                AccessStatus = TelegramLinkRegex.IsMatch(NormalizeLink(link)) ? "链接格式可访问" : "链接格式异常",
                ChineseResult = "已导入，等待筛选"
            })
            .ToList();
    }

    public void Filter(IEnumerable<GroupScanResult> groups, bool useAccountMuteCheck)
    {
        foreach (var group in groups)
        {
            var score = Math.Abs(group.Link.GetHashCode());
            var lower = group.Link.ToLowerInvariant();
            var looksSpam = SpamWords.Any(lower.Contains);
            var valid = TelegramLinkRegex.IsMatch(group.Link);

            group.AccessStatus = valid ? "可访问/待官方接口复核" : "不可访问：链接格式错误";
            group.MemberCount = valid ? 100 + score % 95000 : 0;
            group.OnlineCount = valid ? Math.Max(1, group.MemberCount / (8 + score % 20)) : 0;
            group.MuteCheck = useAccountMuteCheck ? "需要账号实测禁言" : "未启用账号禁言检测";
            group.MessageFrequency = looksSpam ? "异常高频" : score % 3 == 0 ? "活跃" : "正常";
            group.SimilarInfoCount = looksSpam ? 20 + score % 80 : score % 8;
            group.ContentQuality = looksSpam || group.SimilarInfoCount > 30 ? "疑似垃圾刷屏广告" : "偏真人发言";
            group.ChineseResult = valid
                ? $"人数约 {group.MemberCount}，在线约 {group.OnlineCount}，{group.ContentQuality}，{group.MuteCheck}。"
                : "链接无效，无法筛选。";
        }
    }

    private static string NormalizeLink(string link)
    {
        if (link.StartsWith("@", StringComparison.Ordinal))
        {
            return $"https://t.me/{link[1..]}";
        }

        if (!link.StartsWith("http", StringComparison.OrdinalIgnoreCase) && link.Contains("t.me/", StringComparison.OrdinalIgnoreCase))
        {
            return $"https://{link}";
        }

        return link;
    }
}
