using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
    internal class EvolutionTarget : ShareTarget
    {
        private string Subject { get; set; }
        private string Body { get; set; }
        private string Title { get; set; }
        private string[] Attachments { get; set; }
        public EvolutionTarget(string subject, string body, string title, params string[] attachments)
            : base("Evolution", "evolution")
        {
            Subject = subject;
            Body = body;
            Title = title;
            Attachments = attachments;
        }

        public override Task<bool> Invoke
        {
            get
            {
                AsyncHelper.RunSync(async () => await Email.ComposeAsync(new EmailMessage()
                {
                    Subject = Subject ?? Title,
                    Body = Body,
                    Attachments = Attachments?.Select(a => new EmailAttachment(a)).ToList()
                }));
                return Task.FromResult(true);
            }
        }
    }
}
