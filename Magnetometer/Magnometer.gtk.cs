using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class MagnetometerImplementation
    {
        private CancellationTokenSource? _cts;
        private Task? _pollingTask;

        private readonly string? _devicePath;

        public MagnetometerImplementation()
        {
            if (IsSupported)
            {
                // Try to find an iio device with accel
                foreach (var dir in Directory.GetDirectories("/sys/bus/iio/devices/"))
                {
                    if (File.Exists(Path.Combine(dir, "in_magn_x_raw")) &&
                File.Exists(Path.Combine(dir, "in_magn_y_raw")) &&
                File.Exists(Path.Combine(dir, "in_magn_z_raw")))
                    {
                        _devicePath = dir;
                        break;
                    }
                }
            }
        }

        bool PlatformIsSupported =>
            Directory.Exists("/sys/bus/iio/devices/");

        void PlatformStart(SensorSpeed sensorSpeed)
        {
            if (_devicePath == null)
                throw new NotSupportedException("No magnetometer found in /sys/bus/iio/devices");

            _cts = new CancellationTokenSource();
            _pollingTask = Task.Run(() => PollingLoop(sensorSpeed, _cts.Token));
        }

        void PlatformStop()
        {
            _cts?.Cancel();
            _pollingTask?.Wait();
        }

        private void PollingLoop(SensorSpeed sensorSpeed, CancellationToken token)
        {
            // Apply scale if exists
            double scale = 1.0;
            var scalePath = Path.Combine(_devicePath, "in_magn_scale");
            if (File.Exists(scalePath))
                scale = double.Parse(File.ReadAllText(scalePath).Trim(),
                    System.Globalization.CultureInfo.InvariantCulture);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    double x = ReadRaw("in_magn_x_raw") * scale;
                    double y = ReadRaw("in_magn_y_raw") * scale;
                    double z = ReadRaw("in_magn_z_raw") * scale;

                    var data = new MagnetometerData(x, y, z);
                    RaiseReadingChanged(data);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error reading magnetometer : {ex.Message}");
                }

                Thread.Sleep(GetInterval(sensorSpeed)); // ~20Hz, tune according to sensorSpeed
            }
        }

        private double ReadRaw(string filename)
        {
            var path = Path.Combine(_devicePath, filename);
            if (!File.Exists(path)) return 0;
            var text = File.ReadAllText(path).Trim();
            return double.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private int GetInterval(SensorSpeed speed) => speed switch
        {
            SensorSpeed.Default => 200,  // ~5 Hz
            SensorSpeed.UI => 66,   // ~15 Hz
            SensorSpeed.Game => 33,   // ~30 Hz
            SensorSpeed.Fastest => 1,    // As fast as possible
            _ => 100
        };
    }
}
