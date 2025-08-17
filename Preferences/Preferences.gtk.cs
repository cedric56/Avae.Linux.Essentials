using Microsoft.Maui.Essentials;
using Newtonsoft.Json;
using System.Text.Json;

namespace Microsoft.Maui.Storage
{
    class PreferencesImplementation : IPreferences
    {
        private readonly string preferencesFilePath;

        public PreferencesImplementation()
        {
            string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", GtkEssentials.AppName ?? "MyApp");
            preferencesFilePath = Path.Combine(configDir, "preferences.json");

            // Ensure the directory exists
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            // Ensure the file exists
            if (!File.Exists(preferencesFilePath))
            {
                File.WriteAllText(preferencesFilePath, "{}");
            }
        }

        public bool ContainsKey(string key, string name)
        {
            var preferences = LoadPreferences();
            return preferences.ContainsKey(key);
        }

        public void Set<T>(string key, T value, string name)
        {
            var preferences = LoadPreferences();
            preferences[key] =  JsonConvert.SerializeObject(value);
            SavePreferences(preferences);
        }

        public void Clear(string name)
        {
            SavePreferences(new Dictionary<string, string>());
        }

        public T Get<T>(string key, T defaultValue, string name)
        {
            var preferences = LoadPreferences();
            return preferences.ContainsKey(key) ? JsonConvert.DeserializeObject<T>(preferences[key]) : default(T);
        }

        public void Remove(string key, string name)
        {
            var preferences = LoadPreferences();
            if (preferences.Remove(key))
            {
                SavePreferences(preferences);
            }
        }

        private Dictionary<string, string> LoadPreferences()
        {
            if (!File.Exists(preferencesFilePath))
            {
                return new Dictionary<string, string>();
            }

            string json = File.ReadAllText(preferencesFilePath);
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }

        private void SavePreferences(Dictionary<string, string> preferences)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(preferences, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(preferencesFilePath, json);
        }
    }
}

