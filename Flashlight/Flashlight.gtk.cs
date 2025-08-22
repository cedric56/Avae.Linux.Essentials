namespace Microsoft.Maui.Devices
{
    class FlashlightImplementation : IFlashlight
    {
        private readonly string? _ledPath;

        public FlashlightImplementation()
        {
            var names = new string[] { "torch", "led0", "led1" };
            foreach(var name in  names)
            {
                var path = Path.Combine("/sys/class/leds", name, "brightness");
                if (File.Exists(path))
                {
                    _ledPath = path;
                    break;
                }
            }
        }

        public Task<bool> IsSupportedAsync()
        {
            return Task.FromResult(File.Exists(_ledPath));
        }

        public async Task TurnOffAsync()
        {
            if (!await IsSupportedAsync()) 
                return;

            File.WriteAllText(_ledPath!, "1");
        }

        public async Task TurnOnAsync()
        {
            if (!await IsSupportedAsync()) 
                return;
            File.WriteAllText(_ledPath!, "0");
        }
    }
}
