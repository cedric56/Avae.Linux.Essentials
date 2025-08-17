namespace Microsoft.Maui.Devices
{
    public partial class BatteryImplementation : IBattery
    {
        public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.Unknown;

        private const string BatteryPath = "/sys/class/power_supply/";

        public BatteryState State => GetBatteryState();
        public BatteryPowerSource PowerSource => GetPowerSource();
        public double ChargeLevel => GetChargeLevel();

        private BatteryState GetBatteryState()
        {
            var batteryStatusFilePath = Path.Combine(BatteryPath, "BAT0", "status");
            if (!File.Exists(batteryStatusFilePath))
            {
                return BatteryState.NotPresent;
            }

            var batteryStatus = File.ReadAllText(batteryStatusFilePath).Trim().ToLower();
            return batteryStatus switch
            {
                "charging" => BatteryState.Charging,
                "discharging" => BatteryState.Discharging,
                "full" => BatteryState.Full,
                _ => BatteryState.Unknown,
            };
        }

        private BatteryPowerSource GetPowerSource()
        {
            // Determine if the device is plugged in or on battery
            var powerSupplyTypeFilePath = Path.Combine(BatteryPath, "BAT0", "type");
            if (File.Exists(powerSupplyTypeFilePath))
            {
                var type = File.ReadAllText(powerSupplyTypeFilePath).Trim().ToLower();
                return type == "battery" ? BatteryPowerSource.Battery : BatteryPowerSource.AC;
            }

            return BatteryPowerSource.AC;
        }

        private double GetChargeLevel()
        {
            var chargeLevelFilePath = Path.Combine(BatteryPath, "BAT0", "capacity");
            if (!File.Exists(chargeLevelFilePath))
            {
                return 1; // Unknown charge level
            }

            if (int.TryParse(File.ReadAllText(chargeLevelFilePath).Trim(), out int chargeLevel))
            {
                return chargeLevel / 100.0;
            }

            return -1; // Invalid charge level
        }

        FileSystemWatcher watcher;

        void StartBatteryListeners()
        {
            watcher = new FileSystemWatcher("/sys/class/power_supply/BAT0")
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "status"
            };
            watcher.Changed += OnChanged;
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, EventArgs args)
        {
            OnBatteryInfoChanged();
        }

        void StopBatteryListeners()
        {
            watcher.Changed -= OnChanged;
            watcher.EnableRaisingEvents = false;
        }

        void StartEnergySaverListeners()
        {
        }

        void StopEnergySaverListeners()
        {
        }
    }
}
