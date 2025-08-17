namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
    partial class ClipboardImplementation : IClipboard
    {
        public Task SetTextAsync(string text) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        public bool HasText 
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        public Task<string> GetTextAsync()
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        //string GetPasteboardText()
        //=> Pasteboard.ReadObjectsForClasses(
        //        new ObjCRuntime.Class[] { new ObjCRuntime.Class(typeof(NSString)) },
        //        null)?[0]?.ToString();

        void StartClipboardListeners()
            => throw ExceptionUtils.NotSupportedOrImplementedException;

        void StopClipboardListeners()
            => throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
