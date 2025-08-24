using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
    internal class AccelerometerImplementation : AccelerometerImplementationBase
    {
        private CancellationTokenSource? _cts;
        private Task? _pollingTask;

        private readonly string? _devicePath;

        public AccelerometerImplementation()
        {
            // Try to find an iio device with accel
            foreach (var dir in Directory.GetDirectories("/sys/bus/iio/devices/"))
            {
                if (File.Exists(Path.Combine(dir, "in_accel_x_raw")))
                {
                    _devicePath = dir;
                    break;
                }
            }
        }

        public override bool IsSupported => _devicePath is not null;

        protected override void PlatformStart(SensorSpeed sensorSpeed)
        {
            if (_devicePath == null)
                throw new NotSupportedException("No accelerometer found in /sys/bus/iio/devices");

            _cts = new CancellationTokenSource();
            _pollingTask = Task.Run(() => PollingLoop(sensorSpeed, _cts.Token));
        }

        protected async override void PlatformStop()
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
            string xRaw = Path.Combine(_devicePath, "in_accel_x_raw");
            string yRaw = Path.Combine(_devicePath, "in_accel_y_raw");
            string zRaw = Path.Combine(_devicePath, "in_accel_z_raw");

            string xScaleFile = Path.Combine(_devicePath, "in_accel_x_scale");
            string yScaleFile = Path.Combine(_devicePath, "in_accel_y_scale");
            string zScaleFile = Path.Combine(_devicePath, "in_accel_z_scale");

            double xScale = File.Exists(xScaleFile) ? double.Parse(File.ReadAllText(xScaleFile)) : 1.0;
            double yScale = File.Exists(yScaleFile) ? double.Parse(File.ReadAllText(yScaleFile)) : 1.0;
            double zScale = File.Exists(zScaleFile) ? double.Parse(File.ReadAllText(zScaleFile)) : 1.0;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    double x = int.Parse(File.ReadAllText(xRaw)) * xScale;
                    double y = int.Parse(File.ReadAllText(yRaw)) * yScale;
                    double z = int.Parse(File.ReadAllText(zRaw)) * zScale;

                    var data = new AccelerometerData(x, y, z);
                    OnChanged(data);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error reading accelerometer: {ex.Message}");
                }

                Thread.Sleep(GetInterval(sensorSpeed)); // ~20Hz, tune according to sensorSpeed
            }
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
