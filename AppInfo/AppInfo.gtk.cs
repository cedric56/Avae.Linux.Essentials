using Avalonia;
using Avalonia.Styling;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Essentials;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Maui.ApplicationModel
{
    class AppInfoImplementation : IAppInfo
    {
        static readonly Assembly _launchingAssembly = Assembly.GetEntryAssembly();

        public string PackageName => _launchingAssembly.GetAppInfoValue("PackageName") ?? _launchingAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? string.Empty;

        public string Name => _launchingAssembly.GetAppInfoValue("Name") ?? _launchingAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? string.Empty;

        public System.Version Version => _launchingAssembly.GetAppInfoVersionValue("Version") ?? _launchingAssembly.GetName().Version;

        public string VersionString => Version?.ToString() ?? string.Empty;

        public string BuildString => Version.Revision.ToString(CultureInfo.InvariantCulture);

        public AppTheme RequestedTheme
        {
            get
            {
                var theme = Application.Current?.ActualThemeVariant;
                if(theme == null)
                    return AppTheme.Unspecified;

                return theme == ThemeVariant.Light
                    ? AppTheme.Light
                    : AppTheme.Dark;
            }
        }

        public AppPackagingModel PackagingModel => AppPackagingModel.Unpackaged;

        public LayoutDirection RequestedLayoutDirection
        {
            get
            {
                var direction = AvaloniaInterop.GetTopLevel()?.FlowDirection;
                if (direction == null)
                    return LayoutDirection.Unknown;
                return direction == Avalonia.Media.FlowDirection.LeftToRight
                    ? LayoutDirection.LeftToRight
                    : LayoutDirection.RightToLeft;
            }
        }

        public  void ShowSettingsUI()
        {
            if(DeviceInfo.Current is DeviceInfoImplementation implementation)
            {
                if (implementation.Desktop == Desktop.Gnome)
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "gnome-control-center",
                        Arguments = "applications",
                        UseShellExecute = false
                    });
                }
                else if(implementation.Desktop == Desktop.KDE)
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "systemsettings5",
                        Arguments = "applications",
                        UseShellExecute = false
                    });
                }
            }

        //    string[] settingsCommands = {
        //    "gnome-control-center",
        //    "systemsettings5",
        //    "xfce4-settings-manager",
        //    "unity-control-center",
        //    "lxqt-config"
        //};
        //    foreach (var command in settingsCommands)
        //    {
        //            Process process = new Process();

        //            try
        //            {
        //                process.StartInfo = new ProcessStartInfo("xdg-open", command);
        //                process.Start();
        //            }
        //            catch
        //            {

        //            }
        //    }
        }

        internal static string PublisherName => _launchingAssembly.GetAppInfoValue("PublisherName") ?? _launchingAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty;
    }

    static class AppInfoUtils
    {

        static readonly Lazy<bool> _isPackagedAppLazy = new Lazy<bool>(() =>
        {

            return false;
        });

        public static bool IsPackagedApp => _isPackagedAppLazy.Value;

        public static Version GetAppInfoVersionValue(this Assembly assembly, string name)
        {
            if (assembly.GetAppInfoValue(name) is string value && !string.IsNullOrEmpty(value))
                return Version.Parse(value);

            return null;
        }

        public static string GetAppInfoValue(this Assembly assembly, string name) =>
            assembly.GetMetadataAttributeValue("Microsoft.Maui.ApplicationModel.AppInfo." + name);

        public static string GetMetadataAttributeValue(this Assembly assembly, string key)
        {
            foreach (var attr in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
            {
                if (attr.Key == key)
                    return attr.Value;
            }

            return null;
        }

    }
}
