using System.Dynamic;
using System.Reflection.Metadata;
using Newtonsoft.Json;

namespace ShrimpleCmd
{
    public class Settings
    {
        public bool firstTime { set; get; }
        public bool debugMode { set; get; }
        public string internalCommandPrefix { set; get; }
        public bool enforcePrefix { set; get; }

        public Settings(bool first, bool debug, string internalprefix, bool enforcePre)
        {
            this.firstTime = first;
            this.debugMode = debug;
            this.internalCommandPrefix = internalprefix;
            this.enforcePrefix = enforcePre;
        }


        public static Exception UpdateSettings(string configPath)
        {
            try
            {
                string json = File.ReadAllText(configPath);
                Settings currentSettings = new Settings(true, false, "!", true);

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