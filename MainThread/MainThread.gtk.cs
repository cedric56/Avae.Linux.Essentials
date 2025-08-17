using Avalonia.Threading;

namespace Microsoft.Maui.ApplicationModel
{
    public static partial class MainThread
    {
        static bool PlatformIsMainThread =>
            Dispatcher.UIThread.CheckAccess();

        static void PlatformBeginInvokeOnMainThread(Action action)
        {
            Dispatcher.UIThread.Invoke(action);
        }

        internal static T InvokeOnMainThread<T>(Func<T> factory)
        {
            return Dispatcher.UIThread.Invoke(() => factory());
        }
    }
}
