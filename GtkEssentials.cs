using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Media;
using System.Reflection;

namespace Microsoft.Maui.Essentials
{
    public static class LinuxEssentials
    {
        internal static string AppName { get; private set; }
        internal static string LibcvexternPath { get; private set; }
        internal static ICapturePicker? CapturePicker { get; private set; }
        internal static IAccountPicker? AccountPicker { get; private set; }

        internal static ISharePicker? SharePicker { get; private set; }

        public static void Configure(string appName, string libcvexternPath, IAccountPicker? accountPicker = null, ICapturePicker? capturePicker = null, ISharePicker? sharePicker = null)
        {
            GLib.ExceptionManager.UnhandledException += (e) =>
            {
                // Handle unhandled exceptions globally
                Console.WriteLine($"Unhandled exception: {e.ExceptionObject}");
            };

            AppName = appName;
            LibcvexternPath= libcvexternPath;
            AccountPicker = accountPicker;
            CapturePicker = capturePicker;
            SharePicker = sharePicker;
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
    }
}
