using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
    class FlashlightImplementation : IFlashlight
    {
        public Task<bool> IsSupportedAsync()
        {
            return Task.FromResult(false);
        }

        public Task TurnOffAsync()
        {
            throw ExceptionUtils.NotSupportedOrImplementedException;
        }

        public Task TurnOnAsync()
        {
            throw ExceptionUtils.NotSupportedOrImplementedException;
        }
    }
}
