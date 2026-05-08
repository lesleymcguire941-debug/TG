namespace RvvinTelegramTool.Models;

public sealed class GroupRecord
{
    public string Link { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int? Members { get; set; }
    public int? Online { get; set; }
    public string SpeakStatus { get; set; } = "未检测";
    public string Frequency { get; set; } = "未检测";
    public string SameInfoCount { get; set; } = "0";
    public string ContentJudgement { get; set; } = "未检测";
    public string Result { get; set; } = "待筛选";
}
