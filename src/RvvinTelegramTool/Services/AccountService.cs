using System.Text;
using RvvinTelegramTool.Models;

namespace RvvinTelegramTool.Services;

public sealed class AccountService
{
    private static readonly string[] IphoneProfiles = Enumerable.Range(11, 7)
        .SelectMany(series => new[]
        {
            $"iPhone {series} / iOS {Random.Shared.Next(15, 18)}.{Random.Shared.Next(0, 7)}",
            $"iPhone {series} Pro / iOS {Random.Shared.Next(15, 18)}.{Random.Shared.Next(0, 7)}",
            $"iPhone {series} Pro Max / iOS {Random.Shared.Next(15, 18)}.{Random.Shared.Next(0, 7)}"
        })
        .ToArray();

    private readonly AppFolders _folders;

    public AccountService(AppFolders folders) => _folders = folders;

    public IReadOnlyList<TelegramAccount> ImportSessionFiles(IEnumerable<string> fileNames, bool useIosProfile)
    {
        var imported = new List<TelegramAccount>();

        foreach (var fileName in fileNames)
        {
            var destination = Path.Combine(_folders.Sessions, Path.GetFileName(fileName));
            File.Copy(fileName, destination, overwrite: true);

            imported.Add(new TelegramAccount
            {
                Selected = true,
                DisplayName = Path.GetFileNameWithoutExtension(fileName),
                SourceFile = destination,
                DeviceProfile = useIosProfile ? IphoneProfiles[Random.Shared.Next(IphoneProfiles.Length)] : "默认桌面设备参数",
                Status = "已导入",
                RestrictionNote = "等待账号检测",
                ImportedAt = DateTime.Now
            });
        }

        return imported;
    }

    public void RunLocalSafetyCheck(IEnumerable<TelegramAccount> accounts)
    {
        foreach (var account in accounts)
        {
            var name = account.DisplayName.ToLowerInvariant();
            if (name.Contains("spam") || name.Contains("ban") || name.Contains("限制"))
            {
                account.Status = "异常";
                account.RestrictionNote = "中文结果：疑似受限或冻结，请人工登录 Telegram 并通过 @SpamBot 复核。";
                continue;
            }

            if (string.IsNullOrWhiteSpace(account.SourceFile) || !File.Exists(account.SourceFile))
            {
                account.Status = "异常";
                account.RestrictionNote = "中文结果：session 文件不存在，无法检测。";
                continue;
            }

            account.Status = "本地检测通过";
            account.RestrictionNote = "中文结果：本地文件可用；真实冻结/双向/禁言状态需连接 Telegram 官方接口后复核。";
        }
    }

    public string ExportAccounts(IEnumerable<TelegramAccount> accounts)
    {
        var path = Path.Combine(_folders.Exports, $"accounts_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        var lines = accounts.Select(account => string.Join('\t', new[]
        {
            account.Selected ? "已勾选" : "未勾选",
            account.DisplayName,
            account.DeviceProfile,
            account.Status,
            account.RestrictionNote,
            account.SourceFile
        }));
        File.WriteAllLines(path, lines, Encoding.UTF8);
        return path;
    }
}
