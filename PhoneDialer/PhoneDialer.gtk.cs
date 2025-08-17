using System.Diagnostics;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class PhoneDialerImplementation : IPhoneDialer
    {
        public bool IsSupported => true;

        public void Open(string number)
        {
            try
            {
                // Use xdg-open to open the tel URL with the default application
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = $"tel:{number}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                //process.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to open the phone dialer.", ex);
            }
        }
    }
}
