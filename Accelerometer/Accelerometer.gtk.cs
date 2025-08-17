using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices.Sensors
{
    internal class AccelerometerImplementation : AccelerometerImplementationBase
    {
        public override bool IsSupported =>
            false;

        protected override void PlatformStart(SensorSpeed sensorSpeed)
        {
            throw ExceptionUtils.NotSupportedOrImplementedException;
        }

        protected override void PlatformStop()
        {
            throw ExceptionUtils.NotSupportedOrImplementedException;
        }
    }
}
