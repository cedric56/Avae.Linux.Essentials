using System.Diagnostics;
using System.IO.Pipes;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Media;
using System.Reflection;
using System.Text;
using System.Runtime;

namespace Microsoft.Maui.ApplicationModel
{
    public static class Platform
    {
        internal static string LibcvexternPath { get; private set; }
        internal static ICapturePicker? CapturePicker { get; private set; }
        internal static IAccountPicker? AccountPicker { get; private set; }

        internal static ISharePicker? SharePicker { get; private set; }

        static List<Window> _windows = new List<Window>();

        private static void Patch()
        {
            var proc = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(proc.ProcessName);

            if (processes.Length > 1)
            {
                //iterate through all running target applications      
                foreach (Process p in processes)
                {
                    if (p.Id != proc.Id)
                    {
                        var args = Environment.GetCommandLineArgs();
                        var arg = args.FirstOrDefault(a => a.StartsWith($"{AppActionsExtensions.AppActionPrefix}"));
                        if (arg != null)
                        {
                            using var client = new NamedPipeClientStream(".", "mypipe", PipeDirection.Out);
                            client.Connect();
                            byte[] message = Encoding.UTF8.GetBytes(arg);
                            client.Write(message, 0, message.Length);
                            Environment.Exit(0);
                            break;
                        }
                    }
                }
            }
        }

        private static void Register()
        {
            var thread = new Thread(async () =>
            {
                while (true)
                {
                    using var server = new NamedPipeServerStream("mypipe", PipeDirection.InOut);
                    server.WaitForConnection();

                    byte[] buffer = new byte[256];
                    int bytesRead = server.Read(buffer, 0, buffer.Length);
                    var a = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var id = AppActionsExtensions.ArgumentsToId(a);
                    var actions = await AppActions.GetAsync();
                    var action = actions.FirstOrDefault(a => a.Id == id);
                    if (action != null)
                    {
                        OnLaunched(action);
                    }
                }
            });

            thread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="libcvexternPath">YourProject.PathToLib.libcvextern.so</param>
        /// <param name="accountPicker">if you want to redefine accountPicker</param>
        /// <param name="capturePicker">if you want to redefine capturePicker</param>
        /// <param name="sharePicker">if you want to redefine sharePicker</param>
        /// <returns></returns>
        public static AppBuilder UseMauiEssentials(this AppBuilder builder, string libcvexternPath = null, IAccountPicker? accountPicker = null, ICapturePicker? capturePicker = null, ISharePicker? sharePicker = null)
        {
            Patch();
            Register();            
            
            GLib.ExceptionManager.UnhandledException += (e) =>
            {
                // Handle unhandled exceptions globally
                Console.WriteLine($"Unhandled exception: {e.ExceptionObject}");
            };

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
        
        public static void OnLaunched(AppAction a) =>
            AppActions.Current.OnLaunched(a);

        public static void OnActivated(Avalonia.Controls.Window window) =>
        WindowStateManager.Default.OnActivated(window);
    }
}
