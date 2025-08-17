using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
    internal class ExecTarget : ShareTarget
    {
        private string? _attachment;
        private string _execTemplate;
        public ExecTarget(string name, string software, string exec, string? attachment)
            : base(name, software)
        {
            _attachment = attachment;
            _execTemplate = exec;
        }
        public override Task<bool> Invoke
        {
            get
            {
                // Sanitize and replace common placeholders
                string[] placeholders = { "%f", "%F", "%u", "%U", "%s", "%S" };
                foreach (var ph in placeholders)
                {
                    _execTemplate = _execTemplate.Replace(ph, $"\"{_attachment}\"");
                }

                // Remove any remaining unsupported placeholders
                _execTemplate = Regex.Replace(_execTemplate, "%[a-zA-Z]", "");

                // Split command and arguments
                var match = Regex.Match(_execTemplate, @"^(\S+)\s*(.*)$");
                if (!match.Success) throw new InvalidOperationException("Invalid exec string.");

                string command = match.Groups[1].Value;
                string arguments = match.Groups[2].Value;

                // Launch the process
                var startInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false
                };

                Process.Start(startInfo);
                return Task.FromResult(true);
            }
        }
    }
}
