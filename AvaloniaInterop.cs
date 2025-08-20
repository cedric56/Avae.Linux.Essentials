using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Microsoft.Maui.Essentials
{
    internal class AvaloniaInterop
    {
        public static IClassicDesktopStyleApplicationLifetime Desktop
        {
            get
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
                {
                    return desktopLifetime;
                }
                return null!;
            }
        }

        public static TopLevel? GetTopLevel()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                return TopLevel.GetTopLevel(desktopLifetime.MainWindow);
            }
            else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                return TopLevel.GetTopLevel(singleViewPlatform.MainView);
            }

            return TopLevel.GetTopLevel(null);
        }
    }
}
