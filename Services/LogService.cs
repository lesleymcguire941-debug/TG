namespace RvvinTelegramTool.Services;

public sealed class LogService
{
    public event Action<string>? MessageWritten;

    public string CurrentLogFile { get; } = Path.Combine(
        AppFolders.Logs,
        $"Logs-{DateTime.Now:yyyyMMddHHmmss}.txt");

    public void Write(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
        File.AppendAllText(CurrentLogFile, line + Environment.NewLine);
        MessageWritten?.Invoke(line);
    }
}
