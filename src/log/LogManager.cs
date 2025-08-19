using System;
using System.IO;
using ShrimpleCmd.dev;
using ShrimpleCmd.utils;
using Spectre.Console;

namespace ShrimpleCmd.log
{
    public class LogManager
    {
        private static string logDirectory;
        private static string logFilePath;


        public static void SetupLogging()
        {
            try
            {
                logDirectory = Path.Combine(ApplicationSettings.ExecutableLocation, "logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                logFilePath = Path.Combine(logDirectory, $"ShrimpleCmd_{timestamp}.log");

            }
            catch (Exception ex)
            {

                util.println($"FATAL: Could not set up logging directory or file: {ex.Message}");
            }
        }


        public static void WriteToFile(string level, string className, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(logFilePath))
                {

                    return;
                }
                File.AppendAllText(logFilePath, $"{level} [{className}] {message}\n");
            }
            catch (Exception ex)
            {

                if (ApplicationSettings.MainSettings.ShowLoggingOutput)
                {
                    util.println($"Error writing to log file: {ex.Message}");
                }
            }
        }
    }
}
