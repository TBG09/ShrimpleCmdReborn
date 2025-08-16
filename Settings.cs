using System.Dynamic;
using System.Reflection.Metadata;

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
    }
}