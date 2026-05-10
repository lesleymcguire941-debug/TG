namespace RvvinTelegramTool.Models;

public sealed record AiTokenMonitorSummary(
    int TokenQuota,
    int UsedTokens,
    int RemainingTokens,
    decimal UsedCostUsd,
    int TodayTokens,
    int MonthTokens,
    int AlertThreshold,
    string Status);
