using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Microsoft.Maui.Devices;
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
                var direction = GetTopLevel().FlowDirection;
                if (direction == null)
                    return LayoutDirection.Unknown;
                return direction == Avalonia.Media.FlowDirection.LeftToRight
                    ? LayoutDirection.LeftToRight
                    : LayoutDirection.RightToLeft;
            }
        }

        private void Start(string command)
        {
            
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo(command) {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
};
                process.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start process: {ex.Message}");
            }
        }

        public  void ShowSettingsUI()
        {
            if(DeviceInfo.Current is DeviceInfoImplementation implementation && implementation.Distribution == Distribution.Ubuntu)
            {
                Process.Start(new ProcessStartInfo()
                {
                     FileName = "gnome-control-center",
                    Arguments = "applications",
                    UseShellExecute = false
                });
                //Process.Start("gnome-control-center");
                Start("gnome-control-center");
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

        public static TopLevel GetTopLevel()
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
