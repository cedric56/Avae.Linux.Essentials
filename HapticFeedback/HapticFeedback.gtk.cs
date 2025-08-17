using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
    class HapticFeedbackImplementation : IHapticFeedback
    {
        public bool IsSupported => false;

        public void Perform(HapticFeedbackType type)
            => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
