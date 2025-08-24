using Microsoft.Maui.ApplicationModel;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class GyroscopeImplementation : IGyroscope
    {
        private CancellationTokenSource? _cts;
        private Task? _pollingTask;

        private readonly string? _devicePath;

        public GyroscopeImplementation()
        {
            // Try to find an iio device with accel
            foreach (var dir in Directory.GetDirectories("/sys/bus/iio/devices/"))
            {
                if (File.Exists(Path.Combine(dir, "in_anglvel_x_raw")) &&
            File.Exists(Path.Combine(dir, "in_anglvel_y_raw")) &&
            File.Exists(Path.Combine(dir, "in_anglvel_z_raw")))
                {
                    _devicePath = dir;
                    break;
                }
            }
        }

        bool PlatformIsSupported => _devicePath is not null;

        void PlatformStart(SensorSpeed sensorSpeed)
        {
            if (_devicePath == null)
                throw new NotSupportedException("No gyroscope found in /sys/bus/iio/devices");

            _cts = new CancellationTokenSource();
            _pollingTask = Task.Run(() => PollingLoop(sensorSpeed, _cts.Token));
        }

        async void PlatformStop()
        {
            _cts?.Cancel();
            if (_pollingTask != null)
            {
                try
                {
                    await _pollingTask;
                }
                catch (OperationCanceledException) { }
            }
        }

        private void PollingLoop(SensorSpeed sensorSpeed, CancellationToken token)
        {
            double scale = 1.0;
            var scalePath = Path.Combine(_devicePath, "in_anglvel_scale");
            if (File.Exists(scalePath))
            {
                var s = File.ReadAllText(scalePath).Trim();
                scale = double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            }

            while (!token.IsCancellationRequested)
            {
                try
                {
                    double x = ReadRaw("in_anglvel_x_raw") * scale;
                    double y = ReadRaw("in_anglvel_y_raw") * scale;
                    double z = ReadRaw("in_anglvel_z_raw") * scale;

                    var data = new GyroscopeData(x, y, z);
                    RaiseReadingChanged(data);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error reading accelerometer: {ex.Message}");
                }

                Thread.Sleep(GetInterval(sensorSpeed)); // ~20Hz, tune according to sensorSpeed
            }
        }

        private double ReadRaw(string filename)
        {
            var path = Path.Combine(_devicePath, filename);
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
