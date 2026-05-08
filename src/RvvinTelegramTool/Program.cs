using RvvinTelegramTool.Services;
using RvvinTelegramTool.Views;

namespace RvvinTelegramTool;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var appFolders = AppFolders.CreateDefault();
        appFolders.EnsureCreated();

        Application.Run(new MainForm(appFolders));
    }
}
