using Microsoft.Maui.ApplicationModel;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace Microsoft.Maui.Devices.Sensors
{
    partial class GeolocationImplementation : IGeolocation
    {
        private const string GeoClueService = "org.freedesktop.GeoClue2";
        public static async Task<Location?> GetCurrentLocation()
        {
            using var connection = new Connection(Address.System);
            try
            {
                await connection.ConnectAsync();
                var task = new CancellationTokenSource();
                task.CancelAfter(TimeSpan.FromSeconds(2));
                var manager = new OrgFreedesktopGeoClue2ManagerProxy(connection, GeoClueService,
                    "/org/freedesktop/GeoClue2/Manager");
                var clientPath = await manager.CreateClientAsync().WaitAsync(task.Token);
                var client = new OrgFreedesktopGeoClue2ClientProxy(connection, GeoClueService, clientPath);
                await client.SetDesktopIdPropertyAsync("essentials");
                await client.StartAsync();
                
                if (await client.GetActivePropertyAsync())
                {
                    var locationPath = await client.GetLocationPropertyAsync();
                    var location = new OrgFreedesktopGeoClue2LocationProxy(connection, GeoClueService,
                        locationPath);
                    var latitude = await location.GetLatitudePropertyAsync();
                    var longitude = await location.GetLongitudePropertyAsync();
                    var accurancy = await location.GetAccuracyPropertyAsync();
                    return new Location(latitude, longitude, accurancy);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null!;
        }

        /// <summary>
        /// Indicates if currently listening to location updates while the app is in foreground.
        /// </summary>
        public bool IsListeningForeground { get; set; }

        public Task<Location?> GetLastKnownLocationAsync()
        {
            return GetCurrentLocation();
        }

        public Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            return GetCurrentLocation();
        }

        /// <summary>
        /// Starts listening to location updates using the <see cref="Geolocation.LocationChanged"/>
        /// event or the <see cref="Geolocation.ListeningFailed"/> event. Events may only sent when
        /// the app is in the foreground. Requests <see cref="Permissions.LocationWhenInUse"/>
        /// from the user.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
        /// <exception cref="FeatureNotSupportedException">Thrown if listening is not supported on this platform.</exception>
        /// <exception cref="InvalidOperationException">Thrown if already listening and <see cref="IsListeningForeground"/> returns <see langword="true"/>.</exception>
        /// <param name="request">The listening request parameters to use.</param>
        /// <returns><see langword="true"/> when listening was started, or <see langword="false"/> when listening couldn't be started.</returns>
        public Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (IsListeningForeground)
                throw new InvalidOperationException("Already listening to location changes.");

            IsListeningForeground = true;

            return Task.FromResult(true);
        }

        /// <summary>
        /// Stop listening for location updates when the app is in the foreground.
        /// Has no effect when not listening and <see cref="Geolocation.IsListeningForeground"/>
        /// is currently <see langword="false"/>.
        /// </summary>
        public void StopListeningForeground()
        {
            IsListeningForeground = false;
        }
    }
}
