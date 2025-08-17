using System.Diagnostics;
using System.Management;
using Tmds.DBus.Protocol;
using Tmds.DBus.SourceGenerator;

namespace Microsoft.Maui.Devices
{
    public class DeviceInfoImplementation : IDeviceInfo
    {
        public DeviceInfoImplementation()
        {
            var result = ExecuteBashCommand("hostnamectl status");
            var name = Between(result, "Virtualization:", "Operating System:").Trim();
            var results = ParseOsRelease();

            Desktop = GetDesktop();
            if (Desktop.Equals(Desktop.Unknown))
            {
                Desktop = new Desktop(name.ToUpper());
            }
            Distribution = results.distribution;
            VersionString = results.versionstring;
            Version = new Version(results.version);

            if (Desktop == Desktop.WSL)
            {
                Manufacturer = GetProperty("Manufacturer");
                Model = GetProperty("Model");
                Name = GetProperty("Name");
            }
            else
            {
                Manufacturer = ReadFile("/sys/class/dmi/id/sys_vendor");
                Model = ReadFile("/sys/class/dmi/id/product_name");

            }

            var isVirtual = IsVirtual(name);
            Platform = DevicePlatform.Linux;
            Idiom = GetDeviceIdiom();
            DeviceType = isVirtual ? DeviceType.Virtual : DeviceType.Physical;           
        }

        private string GetProperty(string name)
        {
            string psPath = "/mnt/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe";
            try
            {
                if (!File.Exists(psPath))
                {
                    Console.WriteLine($"Error: {psPath} not found. Ensure WSL can access the Windows filesystem.");
                    return string.Empty;
                }

                Process process = new Process();
                process.StartInfo.FileName = psPath;
                process.StartInfo.Arguments = $"-Command \"Get-CimInstance -ClassName Win32_ComputerSystem | Select-Object -ExpandProperty {name}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing PowerShell: {ex.Message}");
            }
            return string.Empty;
        }

        private DeviceIdiom GetDeviceIdiom()
        {
            string chassisPath = "/sys/class/dmi/id/chassis_type";
            string code = File.Exists(chassisPath) ? File.ReadAllText(chassisPath).Trim() : "0";

            return code switch
            {
                "3" or "4" or "6" or "7" => DeviceIdiom.Desktop,
                "8" or "9" or "10" or "14" => DeviceIdiom.Tablet, // could also be Laptop
                "30" => DeviceIdiom.Tablet,
                "31" => DeviceIdiom.Tablet,
                "32" => DeviceIdiom.Tablet,
                "11" => DeviceIdiom.Phone, // Handheld
                _ => DeviceIdiom.Desktop
            };
        }

        private static string ReadFile(string filePath)
        {
            if (File.Exists(filePath))
                return File.ReadAllText(filePath).Trim();

            return string.Empty;
        }

        private static (string prettyname, string version, string versionstring, Distribution distribution) ParseOsRelease()
        {
            Distribution distribution = Distribution.Unknown;
            string prettyname = string.Empty;
            string version = string.Empty;
            string versionstring = string.Empty;
            string osReleaseFile = "/etc/os-release";
            if (File.Exists(osReleaseFile))
            {
                string[] lines = File.ReadAllLines(osReleaseFile);
                foreach (var line in lines)
                {
                    if (line.StartsWith("NAME="))
                    {
                        var distributionName = line.Split('=')[1].Trim('"');
                        distribution = new Distribution(distributionName);
                    }
                    else if(line.StartsWith("PRETTY_NAME="))
                    {
                        prettyname = line.Split('=')[1].Trim('"');
                    }
                    else if (line.StartsWith("VERSION_ID="))
                    {
                        version = line.Split('=')[1].Trim('"');
                    }
                    else if (line.StartsWith("VERSION="))
                    {
                        versionstring = line.Split('=')[1].Trim('"');
                    }
                }
            }
            return (prettyname, version, versionstring, distribution);
        }

        private static Desktop GetDesktop()
        {
            var value = AsyncHelper.RunSync(async () =>
            {
                try
                {
                    using var connection = new Connection(Address.Session);
                    await connection.ConnectAsync();
                    var dbus = new OrgFreedesktopDBusProxy(connection, "org.freedesktop.DBus", "/org/freedesktop/DBus");
                    var names = await dbus.ListNamesAsync();
                    if (names.Contains("org.gnome.SessionManager"))
                    {
                        return Desktop.Gnome;
                    }
                    else if (names.Contains("org.kde.KWin"))
                    {
                        return Desktop.KDE;
                    }
                    else if (names.Contains("org.xfce.SessionManager"))
                    {
                        return Desktop.Xfce;
                    }
                    else if (names.Contains("org.mate.SessionManager"))
                    {
                        return Desktop.Mate;
                    }
                    else if (names.Contains("org.cinnamon.SessionManager"))
                    {
                        return Desktop.Cinnamon;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error determining desktop environment: {ex.Message}");
                }
                return Desktop.Unknown;
            });

            return value;
        }

        public Distribution Distribution { get; set; }
        public Desktop Desktop { get; set; }

        public string Model { get; set; }

        public string Manufacturer { get; set; }

        public string Name { get; set; }

        public string VersionString { get; set; }

        public Version Version { get; set; }

        public DevicePlatform Platform { get; set; }

        public DeviceIdiom Idiom { get; set; }

        public DeviceType DeviceType { get; set; }

        public static string Between(string input, string first, string last)
        {
            return Between(input, first, input.IndexOf(last));
        }

        public static string Between(string input, string first, int post)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                    return string.Empty;

                string output;
                int index = input.IndexOf(first) + first.Length;
                output = input.Substring(index, post - index);
                return output;
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool IsVirtual(string input)
        {
            return input.Contains("VirtualBox") ||
                input.Contains("Oracle") ||
                input.Contains("VMware") ||
                input.Contains("Parallels") ||
                input.Contains("wsl");
        }

        public static string ExecuteBashCommand(string command)
        {
            // according to: https://stackoverflow.com/a/15262019/637142
            // thans to this we will pass everything as one command
            command = command.Replace("\"", "\"\"");

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();

            return proc.StandardOutput.ReadToEnd();
        }
    }

    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
(CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return _myTaskFactory
              .StartNew(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }

        public static void RunSync(Func<Task> func)
        {
            _myTaskFactory
               .StartNew(func)
               .Unwrap()
               .GetAwaiter()
               .GetResult();
        }

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource();

            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                return await task;  // Very important in order to propagate exceptions
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }
    }
}
