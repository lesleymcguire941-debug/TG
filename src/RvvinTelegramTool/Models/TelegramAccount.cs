namespace RvvinTelegramTool.Models;

public sealed class TelegramAccount
{
    public bool Selected { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public string DeviceProfile { get; set; } = string.Empty;
    public string Status { get; set; } = "未检测";
    public string RestrictionNote { get; set; } = "暂无";
    public DateTime ImportedAt { get; set; } = DateTime.Now;
}
