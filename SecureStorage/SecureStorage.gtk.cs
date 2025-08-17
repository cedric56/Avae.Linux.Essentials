using Newtonsoft.Json;
using SecureLocalStorage;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using static SecureLocalStorage.SecureLocalStorage;

namespace Microsoft.Maui.Storage
{
    partial class SecureStorageImplementation : ISecureStorage, IPlatformSecureStorage
    {
        SecureLocalStorageEx storageEx { get; set; }

        public SecureStorageImplementation()
        {
            var config = new DefaultLocalStorageConfig();
            storageEx = new SecureLocalStorageEx(config);
        }

        public class SecureLocalStorageEx
        {
            internal ISecureLocalStorageConfig Config { get; }

            internal Dictionary<string, string> StoredData { get; set; }

            private byte[] Key { get; }

            public int Count => StoredData.Count;

            internal void CreateIfNotExists(string path)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            public SecureLocalStorageEx(ISecureLocalStorageConfig configuration)
            {
                Config = configuration ?? throw new ArgumentNullException("configuration");
                CreateIfNotExists(Config.StoragePath);
                Key = Encoding.UTF8.GetBytes(Config.BuildLocalSecureKey());
                Read();
            }

            internal byte[] EncryptData(string data, byte[] key, DataProtectionScope scope)
            {
                if (data == null)
                {
                    throw new ArgumentNullException("data");
                }

                if (data.Length <= 0)
                {
                    throw new ArgumentException("data");
                }

                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                if (key.Length == 0)
                {
                    throw new ArgumentException("key");
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return ProtectedData.Protect(Encoding.UTF8.GetBytes(data), key, scope);
                }

                return Encoding.UTF8.GetBytes(AESThenHMAC.SimpleEncryptWithPassword(data, Encoding.UTF8.GetString(key)));
            }

            internal string DecryptData(byte[] data, byte[] key, DataProtectionScope scope)
            {
                if (data == null)
                {
                    throw new ArgumentNullException("data");
                }

                if (data.Length == 0)
                {
                    throw new ArgumentException("data");
                }

                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                if (key.Length == 0)
                {
                    throw new ArgumentException("key");
                }

                return AESThenHMAC.SimpleDecryptWithPassword(Encoding.UTF8.GetString(data), Encoding.UTF8.GetString(key));
            }

            internal void Read()
            {
                var path = Path.Combine(Config.StoragePath, "default");
                if (File.Exists(path))
                {
                    var data = DecryptData(File.ReadAllBytes(path), Key, DataProtectionScope.LocalMachine);
                    if (!string.IsNullOrWhiteSpace(data))
                    {
                        StoredData = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                        return;
                    }
                }

                StoredData = new Dictionary<string, string>();
            }

            internal void Write()
            {
                File.WriteAllBytes(Path.Combine(Config.StoragePath, "default"), EncryptData(JsonConvert.SerializeObject(StoredData), Key, DataProtectionScope.LocalMachine));
                Read();
            }

            public void Clear()
            {
                File.Delete(Path.Combine(Config.StoragePath, "default"));
                Read();
            }

            public bool Exists()
            {
                return File.Exists(Path.Combine(Config.StoragePath, "default"));
            }

            public bool Exists(string key)
            {
                return StoredData.ContainsKey(key);
            }

            public string Get(string key)
            {
                if (StoredData.TryGetValue(key, out var value))
                {
                    return JsonConvert.DeserializeObject<string>(value ?? string.Empty);
                }

                return null!;
            }

            public T Get<T>(string key)
            {
                if (StoredData.TryGetValue(key, out var value))
                {
                    return JsonConvert.DeserializeObject<T>(value ?? string.Empty);
                }

                return default(T);
            }

            public IReadOnlyCollection<string> Keys()
            {
                return StoredData.Keys;
            }

            public void Remove(string key)
            {
                StoredData.Remove(key);
                Write();
            }

            public void Set<T>(string key, T data)
            {
                if (Exists(key))
                {
                    Remove(key);
                }

                StoredData.Add(key, JsonConvert.SerializeObject(data));
                Write();
            }
        }

        Task<string> PlatformGetAsync(string key)
        {
            var value =  storageEx.Get(key);
            return Task.FromResult(value);
        }

        Task PlatformSetAsync(string key, string data)
        {
            storageEx.Set(key, data);
            return Task.CompletedTask;
        }

        bool PlatformRemove(string key)
        {
            storageEx.Remove(key);
            return true;
        }

        void PlatformRemoveAll()
        {
            storageEx.Clear();
        }
    }
}
