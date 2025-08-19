using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using ShrimpleCmd;
using ShrimpleCmd.cli;
using ShrimpleCmd.dev;
using ShrimpleCmd.log;
using ShrimpleCmd.utils;

namespace ShrimpleCmd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // was only used for testing, so commented 
            // util.println(ApplicationSettings.ascii_icon);

            // early temp settings init for logging
            Settings settings = new Settings(true, false, "!", true, true, 100, Settings.HistoryUp.UpArrow, Settings.HistoryDown.DownArrow, false);
            ApplicationSettings.MainSettings = settings;


            LogManager.SetupLogging();
            Logger startuplogger = new Logger("StartupEntry", true, true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyLocation = assembly.Location;
            string assemblyFullName = assembly.FullName;
            startuplogger.info($"Starting ShrimpleCmd {ApplicationSettings.Version}.{ApplicationSettings.Build}");
            startuplogger.debug($"Assembly Full Name: {assemblyFullName}");
            startuplogger.debug($"Assembly Location: {assemblyLocation}");


            ApplicationSettings.args = args;
            startuplogger.info("Passed args to ApplicationArgs");
            startuplogger.info("Passing control to Startup.");
            Startup.main();
        }
    }
}