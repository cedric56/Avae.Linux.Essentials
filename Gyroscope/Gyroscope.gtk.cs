using Microsoft.Maui.ApplicationModel;
using System.Runtime.InteropServices.JavaScript;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class GyroscopeImplementation : IGyroscope
    {

        bool PlatformIsSupported => false;

        void PlatformStart(SensorSpeed sensorSpeed)
        {
            throw ExceptionUtils.NotSupportedOrImplementedException;

        }

        void PlatformStop() {
            throw ExceptionUtils.NotSupportedOrImplementedException;

        }
    }
}
