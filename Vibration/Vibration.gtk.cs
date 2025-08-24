using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
    partial class VibrationImplementation : IVibration
    {
        private readonly string _vibratorPath;

        public VibrationImplementation(string vibratorDevice = "vibrator")
        {
            _vibratorPath = Path.Combine("/sys/class/timed_output", vibratorDevice, "enable");
        }

        public bool IsSupported
            => File.Exists(_vibratorPath);

        void PlatformVibrate()
        {
            PlatformVibrate(TimeSpan.FromSeconds(1));
        }

        void PlatformVibrate(TimeSpan duration)
        {
            if (!IsSupported) return;

            try
            {
                // Write duration in milliseconds to trigger vibration
                File.WriteAllText(_vibratorPath, duration.Milliseconds.ToString());
            }
            catch
            {
                // Ignore errors, usually permission issues
            }
        }

        void PlatformCancel()
            {if (!IsSupported) return;

        try
        {
            // Writing 0 usually cancels vibration
            File.WriteAllText(_vibratorPath, "0");
        }
        catch { }
        }
    }
}
