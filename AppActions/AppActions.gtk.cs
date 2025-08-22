using System.Reflection;
using System.Text;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Maui.ApplicationModel
{
    partial class AppActionsImplementation : IAppActions, IPlatformAppActions
    {
        public bool IsSupported =>
            true;

        private IEnumerable<AppAction> _actions;
        public Task<IEnumerable<AppAction>> GetAsync()
        {
            return Task.FromResult(_actions);
        }


        public Task SetAsync(IEnumerable<AppAction> actions)
        {
            _actions = actions;
            var name = Assembly.GetEntryAssembly()?.GetName().Name;
            var dll = Assembly.GetEntryAssembly()?.Location;
            string desktopFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            $".local/share/applications/{name}.desktop");

            string content = $@"[Desktop Entry]
Name={name}
Exec=/usr/bin/dotnet {dll}
Icon=myapp
Type=Application
Categories=Utility;
Actions={string.Join(";", actions.Select(a => a.Title))};";

            foreach (var action in actions)
            {
                var encodedArg = Convert.ToBase64String(Encoding.UTF8.GetBytes(action.Id));
                content += $@"

[Desktop Action {action.Title}]
Name={action.Title}
Exec=/usr/bin/dotnet {dll} ""{AppActionsExtensions.AppActionPrefix + encodedArg}""";
                //--OnlyShowIn=Unity;GNOME;KDE;";
            }

            if (File.Exists(desktopFile))
                File.Delete(desktopFile);

            Directory.CreateDirectory(Path.GetDirectoryName(desktopFile)!);
            File.WriteAllText(desktopFile, content);

            // Make it executable
            System.Diagnostics.Process.Start("chmod", $"+x {desktopFile}");
            return Task.CompletedTask;
        }

        public Task OnLaunched(AppAction a)
        {
            AppActionActivated?.Invoke(null, new AppActionEventArgs(a));
            return Task.CompletedTask;
        }
        public event EventHandler<AppActionEventArgs> AppActionActivated;
        
        
    }
    
    static partial class AppActionsExtensions
    {
        internal const string AppActionPrefix = "XE_APP_ACTIONS-";

        internal static string ArgumentsToId(this string arguments)
        {
            if (arguments?.StartsWith(AppActionPrefix) ?? false)
                return Encoding.Default.GetString(Convert.FromBase64String(arguments.Substring(AppActionPrefix.Length)));

            return default;
        }
    }
}
