using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Microsoft.Maui.Essentials
{
    public static class ProcessHelper
    {
        public static bool XDG_OPEN(string arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            return process.Start();
        }
    }
}
