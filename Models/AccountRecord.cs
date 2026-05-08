namespace RvvinTelegramTool.Models;

public sealed class AccountRecord
{
    public bool Selected { get; set; } = true;
    public string Phone { get; set; } = string.Empty;
    public string Api { get; set; } = "iOS";
    public string Device { get; set; } = DeviceProfileFactory.CreateIosProfile();
    public string SessionFile { get; set; } = string.Empty;
    public string NetworkStatus { get; set; } = "未登录";
    public string AccountStatus { get; set; } = "待检测";
    public string Restriction { get; set; } = "未检测";
    public string Log { get; set; } = string.Empty;
}

public static class DeviceProfileFactory
{
    private static readonly Random Random = new();

    public static string CreateIosProfile()
    {
        var model = Random.Next(11, 18);
        var minor = Random.Next(0, 6);
        return $"iPhone {model} / iOS 17.{minor}";
    }
}
