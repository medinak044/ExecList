using ExecList.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace ExecList;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Add services
        builder.Services.AddSingleton<IExecListServices, ExecListServices>();
        builder.Services.AddSingleton<IConfigService, ConfigService>();

        #if DEBUG
        builder.Logging.AddDebug();
#endif

        // Set inital app window dimensions
        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS
    events.AddWindows(windows =>
    {
        windows.OnWindowCreated(window =>
        {
            var nativeWindow = window as Microsoft.UI.Xaml.Window;

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            // Get display scale
            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
            var screenWidth = displayArea.WorkArea.Width;
            var screenHeight = displayArea.WorkArea.Height;

            // Customize scale here
            double scaleWidth = 0.5;

            int targetWidth = (int)(screenWidth * scaleWidth);

            // Adjust for aspect ratio
            //double aspectRatio = 16.0 / 9.0;
            double aspectRatio = 1;
            int targetHeight = (int)(targetWidth / aspectRatio);

            appWindow.Resize(new Windows.Graphics.SizeInt32
            {
                Width = targetWidth,
                Height = targetHeight
            });
        });
    });
#endif
        });


        return builder.Build();
    }
}
