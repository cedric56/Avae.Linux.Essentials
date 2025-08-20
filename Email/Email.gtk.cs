using Microsoft.Maui.Essentials;
using System.Diagnostics;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class EmailImplementation : IEmail
    {        
        public static Task<bool> Open(string url)
        {
            bool isSuccessful = false;

            try
            {
                isSuccessful = ProcessHelper.XDG_OPEN(url);
            }
            catch
            {
            }

            return Task.FromResult(isSuccessful);
        }

        public bool IsComposeSupported =>
            true;

        Task PlatformComposeAsync(EmailMessage message)
        {
            if (message == null)
            {
                return Open("mailto:");
            }
            else
            {
                var query = new List<string>();
                string attachments = string.Empty;

                if (!string.IsNullOrEmpty(message.Subject))
                    query.Add("subject=" + Uri.EscapeDataString(message.Subject));

                if (!string.IsNullOrEmpty(message.Body))
                    query.Add("body=" + Uri.EscapeDataString(message.Body));

                if (message.Cc?.Any() == true)
                    query.Add("cc=" + Uri.EscapeDataString(string.Join(",", message.Cc)));

                if (message.Bcc?.Any() == true)
                    query.Add("bcc=" + Uri.EscapeDataString(string.Join(",", message.Bcc)));

                // Note: attachments are not officially supported by `mailto:` and usually ignored.
                if (message.Attachments?.Any() == true)
                    query.Add("attach=" + string.Join("&attach=", message.Attachments.Select(a => a.FullPath)));

                var recipients = string.Join(",", message.To?.Select(Uri.EscapeDataString) ?? []);

                var uri = $"mailto:{recipients}";

                if (query.Count > 0)
                    uri += "?" + string.Join("&", query);

                return Open(uri);
            }
        }
    }
}
