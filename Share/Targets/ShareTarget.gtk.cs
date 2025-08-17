using Avalonia.Media;
using Avalonia.Media.Imaging;
using GLib;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
    public abstract class ShareTarget
    {
        static Dictionary<string, Bitmap> Entries = new Dictionary<string, Bitmap>();

        public string Name { get; set; }

        public IImage Icon { get; set; }

        public abstract Task<bool> Invoke { get; }

        public ShareTarget(string name, string software)
        {
            Name = name;
            Icon = GetIcon(software);
        }

        public static IImage GetIcon(string software)
        {
            if (Entries.TryGetValue(software, out var image))
                return image;

            var appIcon = GetAppIconPath(software);
            if (!string.IsNullOrWhiteSpace(appIcon))
            {
                var bitmap = new Bitmap(appIcon);
                Entries.Add(software, bitmap);
                return bitmap;
            }

            if (Gtk.Application.Default is null)
                Gtk.Application.Init();

            var icon = Gtk.IconTheme.Default.LookupIcon(software, 48, Gtk.IconLookupFlags.UseBuiltin);
            if (!string.IsNullOrWhiteSpace(icon?.Filename))
            {
                if (Path.GetExtension(icon.Filename) == ".svg")
                {
                    using (var memory = new MemoryStream())
                    {
                        var pixbuf = icon.LoadIcon();
                        var bytes = pixbuf.SaveToBuffer("png");
                        memory.Write(bytes);
                        memory.Position = 0;

                        var svg = new Bitmap(memory);
                        Entries.Add(software, svg);
                        return svg;
                    }
                }
                var bitmap = new Bitmap(icon!.Filename);
                Entries.Add(software, bitmap);
                return bitmap;
            }

            return null!;
        }

        static string GetAppIconPath(string appName)
        {
            // Directories to search for .desktop files
            string[] desktopDirs = {
            "/usr/share/applications",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/applications")
        };

            // Directories to search for icons
            string[] iconDirs = {
            "/usr/share/icons",
            "/usr/share/pixmaps"
        };

            foreach (var dir in desktopDirs)
            {
                if (!Directory.Exists(dir)) continue;

                foreach (var desktopFile in Directory.GetFiles(dir, "*.desktop"))
                {
                    var lines = File.ReadAllLines(desktopFile);

                    bool nameMatches = lines.Any(l => l.StartsWith("Name=") && l.IndexOf(appName, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (!nameMatches) continue;

                    var iconLine = lines.FirstOrDefault(l => l.StartsWith("Icon="));
                    if (iconLine != null)
                    {
                        string iconName = iconLine.Split('=')[1];

                        // Search for the actual icon file
                        foreach (var iconDir in iconDirs)
                        {
                            foreach (var ext in new[] { ".png", ".svg", ".jpg", ".jpeg", ".xpm" })
                            {
                                var files = Directory.GetFiles(iconDir, iconName + ext, SearchOption.AllDirectories);
                                if (files.Length > 0)
                                    return files[0];
                            }
                        }
                    }
                }
            }

            return null;
        }
    }

}
