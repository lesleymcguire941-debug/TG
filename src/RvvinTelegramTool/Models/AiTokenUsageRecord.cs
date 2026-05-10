namespace RvvinTelegramTool.Models;

public sealed class AiTokenUsageRecord
{
    public bool Selected { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.Now;
    public string Provider { get; set; } = "OpenAI";
    public string Model { get; set; } = "gpt-4o-mini";
    public string Scenario { get; set; } = "账号/群组分析";
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int CachedTokens { get; set; }
    public decimal CostUsd { get; set; }
    public string Note { get; set; } = string.Empty;

    public int TotalTokens => PromptTokens + CompletionTokens + CachedTokens;
}
