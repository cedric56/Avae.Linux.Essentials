using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.ApplicationModel
{
    class MapImplementation : IMap
    {
        private void OpenUrl(string url)
        {
            ProcessHelper.XDG_OPEN(url);
        }

        public Task OpenAsync(Placemark placemark, MapLaunchOptions options)
        {
            var query = Uri.EscapeDataString($"{placemark.Thoroughfare} {placemark.Locality} {placemark.CountryName}");
            var url = $"https://www.google.com/maps/search/?api=1&query={query}";
            OpenUrl(url);
            return Task.CompletedTask;
        }

        public Task<bool> TryOpenAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            return Task.FromResult(true);
        }

        public Task<bool> TryOpenAsync(Placemark placemark, MapLaunchOptions options)
        {
            return Task.FromResult(true);
        }

        public Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
        {
            var url = $"https://www.google.com/maps/search/?api=1&query={latitude},{longitude}";
            OpenUrl(url);
            return Task.CompletedTask;
        }
    }
}

