using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;
using System.Timers;

namespace Microsoft.Maui.Devices
{
    partial class DeviceDisplayImplementation : IDeviceDisplay
    {
        public DeviceDisplayImplementation()
        {
            if (Gtk.Application.Default == null)
                Gtk.Application.Init();
        }

        public void RequestWakeLock()
        {
            // You can implement logic here to prevent the screen from turning off
            // on Linux using xset or other utilities. For simplicity, this is left unimplemented.
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xset",
                        Arguments = keepScreenOn ? "s off -dpms" : "s on +dpms",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting KeepScreenOn: {ex.Message}");
            }
        }

        bool keepScreenOn = false;

        protected override bool GetKeepScreenOn() => keepScreenOn;

        protected override void SetKeepScreenOn(bool keepScreenOn)
        {
            this.keepScreenOn = keepScreenOn;

            if(this.keepScreenOn)
            {
                RequestWakeLock();
            }
            else
            {
                RequestWakeLock();
            }
        }

        protected override DisplayInfo GetMainDisplayInfo()
        {
            var mainMonitor = Monitor;
            var geometry = mainMonitor.Geometry;
            var orientation = geometry.Height > geometry.Width ? DisplayOrientation.Portrait : DisplayOrientation.Landscape;
            var rate = mainMonitor.RefreshRate / 1000; // The value is in milli-Hertz, so a refresh rate of 60Hz is returned as 60000.
            
            return new DisplayInfo(geometry.Width, geometry.Height, DefaultScreen.Resolution, orientation, DisplayRotation.Unknown, rate);
        }

        ScreenChangeDetector _detector = new ScreenChangeDetector();
        static int iScreen = 0;
        protected override void StartScreenMetricsListeners()
        {
            _detector.Start();
            _detector.ScreenChanged += _detector_ScreenChanged;
            DefaultScreen.SizeChanged += DefaultScreenOnSizeChanged;
        }

        private void _detector_ScreenChanged(object? sender, int e)
        {
            iScreen = e;
            OnMainDisplayInfoChanged();
        }

        void DefaultScreenOnSizeChanged(object sender, EventArgs e)
        {
            OnMainDisplayInfoChanged();
        }

        protected override void StopScreenMetricsListeners()
        {
            _detector.ScreenChanged -= _detector_ScreenChanged;
            _detector.Stop();
            DefaultScreen.SizeChanged += DefaultScreenOnSizeChanged;
        }

        public static Gdk.Screen DefaultScreen => Gdk.Screen.Default;

        /// <summary>
        /// A <see cref="Gdk.Visual"/> describes a particular video hardware display format.
        /// It includes information about the number of bits used for each color, the way the bits are translated into an RGB value for display,
        /// and the way the bits are stored in memory. For example, a piece of display hardware might support 24-bit color, 16-bit color, or 8-bit color;
        /// meaning 24/16/8-bit pixel sizes. For a given pixel size, pixels can be in different formats;
        /// for example the “red” element of an RGB pixel may be in the top 8 bits of the pixel, or may be in the lower 4 bits.
        /// There are several standard visuals.
        /// The visual returned by Gdk.Screen.Default.SystemVisual is the system’s default visual,
        /// and the visual returned Gdk.Screen.Default.RgbaVisual should be used for creating windows with an alpha channel.
        ///
        /// Get the system’s default visual for screen .
        /// This is the visual for the root window of the display.
        /// The return value should not be freed.
        /// </summary>
        public static Gdk.Visual SystemVisual => Gdk.Screen.Default.SystemVisual;

        public static double DefaultResolution => DefaultScreen.Resolution;

        /// <summary>
        /// GdkDisplay objects purpose are two fold:
        ///	to manage and provide information about input devices (pointers and keyboards)
        ///	to manage and provide information about the available <see cref="Gdk.Screen"/>s.
        ///	<see cref="Gdk.Display"/>'s are the GDK representation of an X Display, which can be described as a workstation consisting of a keyboard,
        /// a pointing device (such as a mouse) and one or more screens.
        /// It is used to open and keep track of various <see cref="Gdk.Screen"/> objects currently instantiated by the application.
        /// It is also used to access the keyboard(s) and mouse pointer(s) of the display.
        /// </summary>
        public static Gdk.Display DefaultDisplay => Gdk.Display.Default;

        public static Gdk.Monitor CurrentMonitor => DefaultDisplay.GetMonitorAtPoint(0, 0); // TODO: find out aktual mouse position

        public static Gdk.Monitor Monitor
        {
            get
            {
                var monitors = GetMonitors();
                return monitors.ElementAt(iScreen);
            }
            
        }

        public static int CurrentScaleFactor = CurrentMonitor.ScaleFactor;

        public static IEnumerable<Gdk.Monitor> GetMonitors()
        {
            for (var i = 0; i < DefaultDisplay.NMonitors; i++)
            {
                yield return DefaultDisplay.GetMonitor(i);
            }
        }

        public class ScreenChangeDetector
        {
            public event EventHandler<int> ScreenChanged;
            private readonly IEnumerable<Gdk.Monitor> _monitors; // IEnumerable<Gdk.Monitor>
            private readonly System.Timers.Timer _timer;
            private int _lastMonitorIndex = -1;

            public ScreenChangeDetector()
            {
                _monitors = DeviceDisplayImplementation.GetMonitors();
                _timer = new System.Timers.Timer(1000); // Poll every 1 second
                _timer.Elapsed += CheckScreenChange;
                _timer.AutoReset = true;
            }

            public void Start()
            {
                _timer.Start();
            }

            public void Stop()
            {
                _timer.Stop();
            }

            private void CheckScreenChange(object sender, ElapsedEventArgs e)
            {
                // Ensure we're on the GTK thread (required for GDK calls)
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var _gtkWindow = WindowStateManager.Default.GetActiveWindow(false);
                    // Find the monitor containing the window's top-left corner
                    int currentMonitorIndex = -1;
                    for (int i = 0; i < _monitors.Count(); i++)
                    {
                        var monitor = _monitors.ElementAt(i);
                        var geometry = monitor.Geometry;

                        if (_gtkWindow.Position.X >= geometry.X && _gtkWindow.Position.X < geometry.X + geometry.Width &&
                            _gtkWindow.Position.Y >= geometry.Y && _gtkWindow.Position.Y < geometry.Y + geometry.Height)
                        {
                            currentMonitorIndex = i;
                            break;
                        }
                    }

                    if (currentMonitorIndex != _lastMonitorIndex && currentMonitorIndex != -1)
                    {
                        ScreenChanged?.Invoke(this, currentMonitorIndex);
                        _lastMonitorIndex = currentMonitorIndex;
                    }
                });
            }
        }
    }
}
