using System.Diagnostics;

namespace Microsoft.Maui.ApplicationModel
{
    partial class LauncherImplementation
    {
        Task<bool> PlatformCanOpenAsync(Uri uri) =>
            Task.FromResult(true);

        Task<bool> PlatformOpenAsync(Uri uri) =>
            OpenAsync(uri.OriginalString);

        Task<bool> PlatformTryOpenAsync(Uri uri) =>
           Task.FromResult(true);

        Task<bool> PlatformOpenAsync(OpenFileRequest request) =>
            OpenAsync(request.File.FullPath);

        private Task<bool> OpenAsync(string url)
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo("xdg-open", url);
            return Task.FromResult(process.Start());
        }
    }
}
