namespace RvvinTelegramTool.Models;

public sealed class AiTokenMonitorSettings
{
    public int TokenQuota { get; set; } = 1_000_000;
    public int AlertThreshold { get; set; } = 100_000;
}
