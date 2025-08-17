using System.Diagnostics;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class SmsImplementation : ISms
    {
        public bool IsComposeSupported => true;

        Task PlatformComposeAsync(SmsMessage message)
        {
            var recipients = string.Join(",", message.Recipients.Select(r => Uri.EscapeDataString(r)));
            var uri = $"sms:{recipients}";
            if (!string.IsNullOrEmpty(message?.Body))
                uri += "?&body=" + Uri.EscapeDataString(message.Body);
            try
            {
                // Use xdg-open to open the sms URL with the default application
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xdg-open",
                        Arguments = uri,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to send SMS.", ex);
            }
            return Task.CompletedTask;
        }
    }
}

