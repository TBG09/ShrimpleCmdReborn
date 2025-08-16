using System;
using System.IO;
using ShrimpleCmd.dev;
using Newtonsoft.Json;

namespace ShrimpleCmd
{
    public class Main
    {
        // This method can return an Exception if something goes wrong.
        public static Exception createConfig()
        {
            try
            {
                Settings settings3 = new Settings(true);
                File.Delete(ApplicationSettings.ConfigLocation);
                File.Create(ApplicationSettings.ConfigLocation);
                File.WriteAllText(ApplicationSettings.ConfigLocation, JsonConvert.SerializeObject(settings3, Formatting.Indented));
                return null;
            }
            catch (Exception ex)
            {

                return ex;
            }
        }
        
        public static void main()
        {
            Settings settings;
            string settingsContent;

            utils.println("Performing checks...");
            
            try
            {

                if (File.Exists(ApplicationSettings.ConfigLocation))
                {
                    utils.println("Loading settings.");
                    settingsContent = File.ReadAllText(ApplicationSettings.ConfigLocation);
                    settings = JsonConvert.DeserializeObject<Settings>(settingsContent);
                }
                else
                {

                    utils.println("Settings doesn't exist, creating and setting default values.");
                    settings = new Settings(true);
                    File.WriteAllText(ApplicationSettings.ConfigLocation, JsonConvert.SerializeObject(settings, Formatting.Indented));
                }
            }

            catch (JsonReaderException ex)
            {
                utils.println("Exception Occured\n Invalid json format, do you want to reset settings?(Y/N)");
                string answer = Console.ReadLine().ToLower();

                if (answer == "y" || answer == "yes")
                {
                    // This block handles the "yes" case.

                    Exception result = createConfig();

                    if (result != null)
                    {
                        utils.println($"Chained Exception: {result.GetBaseException}");
                        utils.println("Tempoary settings assigned.");

                        // You must set settings here if you want it to be valid
                        // It will get a temporary settings object.
                        settings = new Settings(true); 
                    }
                    else
                    {
                        utils.println("Successfully made new config file.");
                        // settings is not assigned here, you would need to read the new file to assign it.
                        // For now, let's just make sure it's set.
                        settings = new Settings(true);
                    }
                }
                else // This handles "n", "no", and any other input.
                {
                    utils.println("Assuming no. Using temporary settings.");
                    settings = new Settings(true);
                }
            }

            catch (Exception ex)
            {
                utils.println("An unknown exception occurred while loading/creating settings.");
                utils.println("Creating default settings instead.");
                utils.println($"Error details: {ex.Message}");

                settings = new Settings(true);
            }
        }
    }
}