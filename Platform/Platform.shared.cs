using Avalonia;
using Avalonia.Controls;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Media;
using System.Reflection;

namespace Microsoft.Maui.ApplicationModel
{
    public static class Platform
    {
        internal static string AppName { get; private set; }
        internal static string LibcvexternPath { get; private set; }
        internal static ICapturePicker? CapturePicker { get; private set; }
        internal static IAccountPicker? AccountPicker { get; private set; }

        internal static ISharePicker? SharePicker { get; private set; }

        static List<Window> _windows = new List<Window>();

        public static AppBuilder UseMauiEssentials(this AppBuilder builder, string appName, string libcvexternPath = null, IAccountPicker? accountPicker = null, ICapturePicker? capturePicker = null, ISharePicker? sharePicker = null)
        {
            GLib.ExceptionManager.UnhandledException += (e) =>
            {
                // Handle unhandled exceptions globally
                Console.WriteLine($"Unhandled exception: {e.ExceptionObject}");
            };

            AppName = appName;
            LibcvexternPath = GetLibCVExternPath(libcvexternPath);
            AccountPicker = accountPicker;
            CapturePicker = capturePicker;
            SharePicker = sharePicker;

            Window.GotFocusEvent.AddClassHandler(typeof(Window), (sender, args) =>
            {
                var window = (Window)sender!;
                OnActivated(window);
            });
            Window.WindowOpenedEvent.AddClassHandler(typeof(Window), (sender, args) =>
            {
                var window = (Window)sender!;
                if (!_windows.Contains(window))
                {
                    _windows.Add(window);
                    OnActivated(window);
                }
            });
            Window.WindowClosedEvent.AddClassHandler(typeof(Window), (sender, _) =>
            {
                var window = (Window)sender!;
                _windows.Remove(window);
                if (_windows.Count > 0)
                    OnActivated(_windows.Last());
            });
            return builder;
        }

        /// <summary>
        /// Example libcvextern.so is in a folder Native
        /// YourProject.Native.libcvextern.so
        /// </summary>
        /// <param name="embeddedRessourcePath">YourProject.PathToLib.libcvextern.so</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetLibCVExternPath(string embeddedRessourcePath)
        {
            if (string.IsNullOrWhiteSpace(embeddedRessourcePath))
                return string.Empty;

            string tempPath = Path.Combine(Path.GetTempPath(), "libcvextern.so");
            using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(embeddedRessourcePath);// "Microsoft.Maui.Essentials.Native.libcvextern.so");
            if (stream is null)
                throw new Exception("Enable to find embedded ressource");
            using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                stream?.CopyTo(fs);
            }
            return tempPath;
        }

        public static void OnActivated(Avalonia.Controls.Window window) =>
        WindowStateManager.Default.OnActivated(window);
    }
}
