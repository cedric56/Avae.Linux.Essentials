using System.Diagnostics;

namespace Microsoft.Maui.Essentials
{
    public static class ProcessHelper
    {
        public static async Task<(int exitcode, string error, string result)> ExecuteProcess(string filename, string? arguments = null!)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (!process.Start())
                return (1, "Failed to execute process", string.Empty);

            // Read output and error streams in parallel to avoid deadlocks
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();
            await Task.WhenAny(outputTask, errorTask);

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                Console.WriteLine(error);
            }

            return (process.ExitCode, error, output);
        }

        public static async Task<bool> IsProgramInstalled(string command)
        {
            var response = await ExecuteProcess("which", command);
            return !string.IsNullOrWhiteSpace(response.result);
        }
    }
}
