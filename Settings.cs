using System.Dynamic;
using System.Reflection.Metadata;
using Newtonsoft.Json;

namespace ShrimpleCmd
{
    public class Settings
    {
        public bool firstTime { set; get; }
        public bool debugMode { set; get; }

        public Settings(bool firstTime)
        {
            this.firstTime = firstTime;
        }
        // easy way to retreive debug instead of typing the entire if statement
        public bool getDebug(Settings settingsInstance)
        {
            return settingsInstance.debugMode;
        }

        public static Exception UpdateSettings(string configPath, Settings settings)
        {
            try
            {
                string json = File.ReadAllText(configPath);
                Settings currentSettings = settings;

                JsonConvert.PopulateObject(json, currentSettings);
                File.WriteAllText(configPath, JsonConvert.SerializeObject(currentSettings, Formatting.Indented));
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}