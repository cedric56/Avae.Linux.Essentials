using System.Diagnostics;
using System.Text;

namespace Microsoft.Maui.Media
{
    partial class TextToSpeechImplementation : ITextToSpeech
    {
        Task<IEnumerable<Locale>> PlatformGetLocalesAsync()
        {
            List<Locale> locales = new List<Locale>();

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "espeak",
                    Arguments = "--voices",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                using var reader = process.StandardOutput;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip the header line
                    if (line.StartsWith("Voice name"))
                        continue;

                    // Parse the line (this assumes a specific format)
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        var locale = new Locale(parts[1], string.Join(" ", parts[3..]), parts[3], parts[0]);
                        locales.Add(locale);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error retrieving locales: {ex.Message}");
            }

            return Task.FromResult(locales.AsEnumerable());
        }

        Task PlatformSpeakAsync(string text, SpeechOptions? options = null, CancellationToken cancelToken = default)
        {
            if (options == null)
            {
                try
                {
                    // Run the eSpeak command to convert text to speech
                    Process.Start("espeak", $"\"{text}\"");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in Text-to-Speech: {ex.Message}");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(options.Locale?.Language))
                    throw new ArgumentException("Locale language must be specified.", nameof(options.Locale));

                var builder = new StringBuilder();
                builder.Append($"\"{text}\"");
                if (options.Locale != null) builder.Append($" -v {options.Locale.Language}");
                if (options.Volume != null) SetVolume(Convert.ToInt32(options.Volume));
                if (options.Pitch != null) builder.Append($" -p {options.Pitch}");

                try
                {
                    // Run the eSpeak command to convert text to speech
                    Process.Start("espeak", builder.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in Text-to-Speech: {ex.Message}");
                }
            }

            return Task.CompletedTask;
        }

        // Method to set system volume using pactl
        private void SetVolume(int volume)
        {
            try
            {
                // Volume level is adjusted from 0 to 100%
                string command = $"pactl set-sink-volume @DEFAULT_SINK@ {volume}%";
                Process.Start("bash", $"-c \"{command}\"");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting volume: {ex.Message}");
            }
        }
    }
}
