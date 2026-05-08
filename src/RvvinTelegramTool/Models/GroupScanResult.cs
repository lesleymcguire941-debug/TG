namespace RvvinTelegramTool.Models;

public sealed class GroupScanResult
{
    public bool Selected { get; set; }
    public string Link { get; set; } = string.Empty;
    public string AccessStatus { get; set; } = "待筛选";
    public int MemberCount { get; set; }
    public int OnlineCount { get; set; }
    public string MuteCheck { get; set; } = "待检测";
    public string MessageFrequency { get; set; } = "未知";
    public int SimilarInfoCount { get; set; }
    public string ContentQuality { get; set; } = "未判断";
    public string ChineseResult { get; set; } = "待导入后筛选";
}
