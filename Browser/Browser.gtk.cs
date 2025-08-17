using System.Diagnostics;

namespace Microsoft.Maui.ApplicationModel
{
    class BrowserImplementation : IBrowser
    {
        public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options)
        {
            bool isSuccessful = false;

            Process process = new Process();

            try
            {
                process.StartInfo = new ProcessStartInfo("xdg-open", uri.OriginalString);
                isSuccessful = process.Start();
            }
            catch
            {
               
            }

            return Task.FromResult(isSuccessful);
        }
    }
}
