using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class CompassImplementation : ICompass
    {
        bool PlatformIsSupported => Magnetometer.IsSupported;

        void PlatformStart(SensorSpeed sensorSpeed, bool applyLowPassFilter)
        {
            Magnetometer.Start(sensorSpeed);
            Magnetometer.ReadingChanged += Magnetometer_ReadingChanged;
        }

        private void Magnetometer_ReadingChanged(object? sender, MagnetometerChangedEventArgs e)
        {
            // Compute heading in degrees from X, Y components
            var x = e.Reading.MagneticField.X;
            var y = e.Reading.MagneticField.Y;

            double heading = Math.Atan2(y, x) * (180.0 / Math.PI);

            // Normalize to 0–360
            if (heading < 0) heading += 360;

            ReadingChanged?.Invoke(this, new CompassChangedEventArgs(new CompassData(heading)));
        }

        void PlatformStop()
        {            
            Magnetometer.ReadingChanged -= Magnetometer_ReadingChanged;
            Magnetometer.Stop();
        }
    }
}
