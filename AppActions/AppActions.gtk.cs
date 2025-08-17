using Avalonia.Controls;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Maui.ApplicationModel
{
    partial class AppActionsImplementation : IAppActions
    {
        private IEnumerable<AppAction> _actions;

        public bool IsSupported =>
            true;

        public Task<IEnumerable<AppAction>> GetAsync()
        {
            return Task.FromResult(_actions);
        }


        public Task SetAsync(IEnumerable<AppAction> actions)
        {
            _actions = actions;

            var icons = new TrayIcons();
            foreach (var action in actions)
            {
                var menu = new NativeMenu();
                menu.Items.Add(new NativeMenuItem(action.Title));

                var tray = new TrayIcon()
                {
                    Icon = action.Icon is null ? null : new WindowIcon(action.Icon),
                    Menu = menu,
                    IsVisible = true
                };
                tray.Clicked += (s, e) => {
                    AppActionActivated?.Invoke(this, new AppActionEventArgs(action));
                };
                icons.Add(tray);
            }

            TrayIcon.SetIcons(Avalonia.Application.Current, icons);

            return Task.CompletedTask;
        }


        public event EventHandler<AppActionEventArgs> AppActionActivated;

    }
}
