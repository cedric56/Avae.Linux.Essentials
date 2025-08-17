using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Storage
{
    partial class  FileSystemImplementation : IFileSystem
    {

        static string CleanPath(string path) =>
            string.Join("_", path.Split(Path.GetInvalidFileNameChars()));

        static string AppSpecificPath =>
            Path.Combine(CleanPath(AppInfoImplementation.PublisherName), CleanPath(AppInfo.PackageName));

        string PlatformCacheDirectory
        {
            get
            {
              var path=   Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppSpecificPath, "Cache");
                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
            }

        string PlatformAppDataDirectory
        {
            get
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppSpecificPath, "Data");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        Task<Stream> PlatformOpenAppPackageFileAsync(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            var file = FileSystemUtils.PlatformGetFullAppPackageFilePath(filename);

            return Task.FromResult((Stream)File.OpenRead(file));

        }

        Task<bool> PlatformAppPackageFileExistsAsync(string filename)
        {
            var file = FileSystemUtils.PlatformGetFullAppPackageFilePath(filename);

            return Task.FromResult(File.Exists(file));
        }

    }

    static partial class FileSystemUtils
    {

        public static bool AppPackageFileExists(string filename)
        {
            var file = PlatformGetFullAppPackageFilePath(filename);

            return File.Exists(file);
        }

        public static string PlatformGetFullAppPackageFilePath(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            filename = NormalizePath(filename);

            string root;

            root = AppContext.BaseDirectory;

            return Path.Combine(root, filename);
        }

    }

    public partial class FileBase
    {

        static string PlatformGetContentType(string extension) =>
            MimeHelper.GetMimeType(extension);

        internal void Init(FileBase file)
        { }

        internal virtual Task<Stream> PlatformOpenReadAsync() =>
            Task.FromResult<Stream>(new FileStream(FullPath, FileMode.Open, FileAccess.Read));

        void PlatformInit(FileBase file)
        { }

    }
}
