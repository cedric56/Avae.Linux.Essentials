using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;

namespace Microsoft.Maui.Media
{
    partial class ScreenshotImplementation : IScreenshot
    {
        public bool IsCaptureSupported =>
           true;

        public Task<IScreenshotResult> CaptureAsync()
        {
            var view = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            IScreenshotResult result = new ScreenshotResult(view.MainWindow);
            return Task.FromResult(result);
        }

        public static Task<MemoryStream> CaptureToStreamAsync(Visual visual, ScreenshotFormat format, int quality)
        {
            var pixelSize = new PixelSize((int)visual.Bounds.Width, (int)visual.Bounds.Height);
            var dpi = new Vector(96, 96);
            var bitmap = new RenderTargetBitmap(pixelSize, dpi);

            bitmap.Render(visual);

            var stream = new MemoryStream();
            switch (format)
            {
                case ScreenshotFormat.Png:
                    bitmap.Save(stream, quality);
                    break;
                default:
                    throw new NotSupportedException("Unsupported format.");
            }

            stream.Position = 0;
            return Task.FromResult(stream);
        }

    }

    partial class ScreenshotResult : IScreenshotResult
    {
        Visual visual;

        internal ScreenshotResult(Visual visual)
        {
            this.visual = visual;
        }

        async Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format, int quality) =>
            await ScreenshotImplementation.CaptureToStreamAsync(visual, format, quality);

        public async Task PlatformCopyToAsync(Stream destination, ScreenshotFormat format, int quality)
        {
            var sourceStream = await PlatformOpenReadAsync(format, quality);
            await sourceStream.CopyToAsync(destination);
        }
    }
}
