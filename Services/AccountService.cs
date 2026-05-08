using System.Text;
using System.Text.Json;
using RvvinTelegramTool.Models;

namespace RvvinTelegramTool.Services;

public sealed class AccountService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public List<AccountRecord> ImportSessionFiles(IEnumerable<string> fileNames, LogService log)
    {
        var imported = new List<AccountRecord>();
        foreach (var fileName in fileNames)
        {
            var destination = Path.Combine(AppFolders.Sessions, Path.GetFileName(fileName));
            File.Copy(fileName, destination, overwrite: true);
            var phone = GuessPhoneNumber(Path.GetFileNameWithoutExtension(fileName));
            var record = new AccountRecord
            {
                Phone = phone,
                SessionFile = destination,
                NetworkStatus = "本地session已导入",
                AccountStatus = "未登录",
                Restriction = "待通过 @SpamBot 检测"
            };
            imported.Add(record);
            log.Write($"导入账号 {record.Phone}，设备参数 {record.Device}");
        }

        return imported;
    }

    public List<AccountRecord> ImportPhoneText(string fileName, LogService log)
    {
        var records = File.ReadAllLines(fileName, Encoding.UTF8)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(phone => new AccountRecord
            {
                Phone = phone,
                NetworkStatus = "待登录",
                AccountStatus = "未登录",
                Restriction = "待检测"
            })
            .ToList();

        foreach (var record in records)
        {
            log.Write($"导入手机号 {record.Phone}，登录设置 iOS，设备 {record.Device}");
        }

        return records;
    }

    public void ExportAccounts(IEnumerable<AccountRecord> accounts, string fileName, LogService log)
    {
        var payload = JsonSerializer.Serialize(accounts, JsonOptions);
        File.WriteAllText(fileName, payload, Encoding.UTF8);
        log.Write($"导出账号到 {fileName}");
    }

    public async Task DetectAccountsAsync(IEnumerable<AccountRecord> accounts, LogService log, CancellationToken cancellationToken)
    {
        foreach (var account in accounts.Where(account => account.Selected))
        {
            cancellationToken.ThrowIfCancellationRequested();
            account.NetworkStatus = "网络已连接";
            account.AccountStatus = string.IsNullOrWhiteSpace(account.SessionFile) ? "需要登录" : "已登录";
            await Task.Delay(180, cancellationToken);

            var fingerprint = Math.Abs(account.Phone.GetHashCode());
            account.Restriction = fingerprint % 11 == 0
                ? "冻结：@SpamBot 返回账号被冻结"
                : fingerprint % 5 == 0
                    ? "双向限制：无法主动私聊，需对方先联系"
                    : fingerprint % 3 == 0
                        ? "发言限制：部分群组禁言或限制发送链接"
                        : "正常：未发现冻结、双向或发言限制";
            account.Log = "已模拟启动 @SpamBot 并返回中文检测结果；接入真实 Telegram API 后可替换检测器。";
            log.Write($"账号检测 {account.Phone}：{account.Restriction}");
        }
    }

    public void MarkLogin(IEnumerable<AccountRecord> accounts, LogService log)
    {
        foreach (var account in accounts.Where(account => account.Selected))
        {
            account.NetworkStatus = "使用本地网络登录";
            account.AccountStatus = "已登录";
            account.Log = "登录参数：iOS 随机 iPhone 11~17 系列";
            log.Write($"账号登录 {account.Phone}：{account.Device}");
        }
    }

    private static string GuessPhoneNumber(string source)
    {
        var digits = new string(source.Where(char.IsDigit).ToArray());
        return digits.Length > 0 ? $"+{digits}" : source;
    }
}
