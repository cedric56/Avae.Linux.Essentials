using Newtonsoft.Json;

namespace Microsoft.Maui.Devices.Sensors
{
    class GeocodingImplementation : IGeocoding
    {
        public async Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            // Nominatim reverse geocoding URL
            string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&addressdetails=1";

            // Make the request to the Nominatim API
            var response = await client.GetStringAsync(url);

            // Parse the JSON response
            var data = JsonConvert.DeserializeObject<Root>(response);

            // Return the full address (can also return specific components)
            return new List<Placemark>()
                {
                    new Placemark()
                {
                    CountryName = data.address.country,
                    CountryCode = data.address.country_code,
                    Location = new Location() { Latitude = data.lat, Longitude = data.lon },
                    FeatureName = data.address.road,
                    Locality = data.address.village,
                    PostalCode = data.address.postcode,

                }
            };
        }

        public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            // Nominatim request URL
            string url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";

            // Make the request to the Nominatim API
            var response = await client.GetStringAsync(url);

            // Parse the JSON response
            var data = JsonConvert.DeserializeObject<IEnumerable<NominatimResponse>>(response);
            return data.Select(d =>
            {
                return new Location()
                {
                    Latitude =  d.Lat,
                    Longitude = d.Lon
                };

            }).ToList();
        }

        public class Root
        {
            public int place_id { get; set; }
            public string licence { get; set; }
            public string osm_type { get; set; }
            public int osm_id { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
            public string @class { get; set; }
            public string type { get; set; }
            public int place_rank { get; set; }
            public double importance { get; set; }
            public string addresstype { get; set; }
            public string name { get; set; }
            public string display_name { get; set; }
            public Address address { get; set; }
            public List<string> boundingbox { get; set; }
        }

        public class Address
        {
            public string road { get; set; }
            public string hamlet { get; set; }
            public string village { get; set; }
            public string municipality { get; set; }
            public string county { get; set; }

            [JsonProperty("ISO3166-2-lvl6")]
            public string ISO31662lvl6 { get; set; }
            public string state { get; set; }

            [JsonProperty("ISO3166-2-lvl4")]
            public string ISO31662lvl4 { get; set; }
            public string region { get; set; }
            public string postcode { get; set; }
            public string country { get; set; }
            public string country_code { get; set; }
        }

        public class NominatimResponse
        {
            [JsonProperty("lat")]
            public double Lat { get; set; }

            [JsonProperty("lon")]
            public double Lon { get; set; }
        }
    }
}
