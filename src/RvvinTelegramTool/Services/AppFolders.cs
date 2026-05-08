namespace RvvinTelegramTool.Services;

public sealed class AppFolders
{
    private AppFolders(string root)
    {
        Root = root;
        Accounts = Path.Combine(root, "accounts");
        Sessions = Path.Combine(root, "sessions");
        Imports = Path.Combine(root, "imports");
        Exports = Path.Combine(root, "exports");
        Logs = Path.Combine(root, "logs");
        Settings = Path.Combine(root, "settings");
    }

    public string Root { get; }
    public string Accounts { get; }
    public string Sessions { get; }
    public string Imports { get; }
    public string Exports { get; }
    public string Logs { get; }
    public string Settings { get; }

    public static AppFolders CreateDefault()
    {
        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RvvinTelegramTool");
        return new AppFolders(root);
    }

    public void EnsureCreated()
    {
        foreach (var folder in new[] { Root, Accounts, Sessions, Imports, Exports, Logs, Settings })
        {
            Directory.CreateDirectory(folder);
        }
    }

    public string NewLogPath(string prefix) => Path.Combine(Logs, $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
}
