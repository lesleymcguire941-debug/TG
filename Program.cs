using RvvinTelegramTool.Services;
using RvvinTelegramTool.UI;

namespace RvvinTelegramTool;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        AppFolders.EnsureCreated();
        Application.Run(new MainForm());
    }
}
