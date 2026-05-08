namespace RvvinTelegramTool.Services;

public static class AppFolders
{
    public static string Root { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RvvinTelegramTool");

    public static string Sessions { get; } = Path.Combine(Root, "sessions");
    public static string Accounts { get; } = Path.Combine(Root, "accounts");
    public static string Imports { get; } = Path.Combine(Root, "imports");
    public static string Exports { get; } = Path.Combine(Root, "exports");
    public static string Logs { get; } = Path.Combine(Root, "logs");
    public static string GroupReports { get; } = Path.Combine(Root, "group-reports");
    public static string Settings { get; } = Path.Combine(Root, "settings");

    public static void EnsureCreated()
    {
        foreach (var folder in new[] { Root, Sessions, Accounts, Imports, Exports, Logs, GroupReports, Settings })
        {
            Directory.CreateDirectory(folder);
        }
    }
}
