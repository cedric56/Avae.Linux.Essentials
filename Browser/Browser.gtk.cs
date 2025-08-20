using Microsoft.Maui.Essentials;
using System.Diagnostics;

namespace Microsoft.Maui.ApplicationModel
{
    class BrowserImplementation : IBrowser
    {
        public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options)
        {
            bool isSuccessful = false;

            try
            {
                isSuccessful = ProcessHelper.XDG_OPEN(uri.OriginalString);
            }
            catch
            {
               
            }

            return Task.FromResult(isSuccessful);
        }
    }
}
