using Avalonia.Controls;

namespace Microsoft.Maui.Devices
{
    public partial class BatteryImplementation : IBattery
    {
        public BatteryImplementation()
        {
            if(Directory.Exists("/sys/class/power_supply"))
            {
                var directories = Directory.GetDirectories("/sys/class/power_supply");

                foreach (var directory in directories)
                {
                    string normalizedPath = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    var name = Path.GetFileName(normalizedPath);
                    if (name is not null)
                    {
                        if (name.ToUpper().StartsWith("BAT"))
                        {
                            BatteryPath = directory;
                        }
                    }
                }
            }
        }

        public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.Unknown;

        private readonly string BatteryPath;

        public BatteryState State => GetBatteryState();
        public BatteryPowerSource PowerSource => GetPowerSource();
        public double ChargeLevel => GetChargeLevel();

        private BatteryState GetBatteryState()
        {
            if (Directory.Exists(BatteryPath))
            {
                var batteryStatusFilePath = Path.Combine(BatteryPath, "status");
                if (!File.Exists(batteryStatusFilePath))
                {
                    return BatteryState.NotPresent;
                }
                Console.WriteLine(batteryStatusFilePath);
                var batteryStatus = File.ReadAllText(batteryStatusFilePath).Trim().ToLower();
                return batteryStatus switch
                {
                    "charging" => BatteryState.Charging,
                    "discharging" => BatteryState.Discharging,
                    "full" => BatteryState.Full,
                    _ => BatteryState.Unknown,
                };
            }
            return BatteryState.Full;
        }

        private BatteryPowerSource GetPowerSource()
        {
            if (Directory.Exists(BatteryPath))
            {
                // Determine if the device is plugged in or on battery
                var powerSupplyTypeFilePath = Path.Combine(BatteryPath, "type");
                if (File.Exists(powerSupplyTypeFilePath))
                {
                    Console.WriteLine(powerSupplyTypeFilePath);
                    var type = File.ReadAllText(powerSupplyTypeFilePath).Trim().ToLower();
                    return type == "battery" ? BatteryPowerSource.Battery : BatteryPowerSource.AC;
                }
            }

            return BatteryPowerSource.AC;
        }

        private double GetChargeLevel()
        {
            if (Directory.Exists(BatteryPath))
            {
                var chargeLevelFilePath = Path.Combine(BatteryPath, "capacity");
                if (!File.Exists(chargeLevelFilePath))
                {
                    return 1; // Unknown charge level
                }

                Console.WriteLine(chargeLevelFilePath);
                if (int.TryParse(File.ReadAllText(chargeLevelFilePath).Trim(), out int chargeLevel))
                {
                    return chargeLevel / 100.0;
                }
            }

            return -1; // Invalid charge level
        }

        FileSystemWatcher watcher;

        void StartBatteryListeners()
        {
            if (Directory.Exists(BatteryPath))
            {
                watcher = new FileSystemWatcher(BatteryPath)
                {
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = "status"
                };
                watcher.Changed += OnChanged;
                watcher.EnableRaisingEvents = true;
            }
        }

        private void OnChanged(object sender, EventArgs args)
        {
            OnBatteryInfoChanged();
        }

        void StopBatteryListeners()
        {
            if (watcher is not null)
            {
                watcher.Changed -= OnChanged;
                watcher.EnableRaisingEvents = false;
            }
        }

        void StartEnergySaverListeners()
        {
        }

        void StopEnergySaverListeners()
        {
        }
    }
}
