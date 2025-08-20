using Microsoft.Maui.Essentials;
using System.Diagnostics;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class PhoneDialerImplementation : IPhoneDialer
    {
        public bool IsSupported => true;

        public void Open(string number)
        {
            try
            {
                ProcessHelper.XDG_OPEN($"tel:{number}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Unable to open the phone dialer.", ex);
            }
        }
    }
}
