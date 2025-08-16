using ShrimpleCmd.dev;
using Newtonsoft.Json;
using Spectre.Console;

namespace ShrimpleCmd
{
    public class Startup
    {
        public static string configCheck;

        public static Exception createConfig()
        {
            try
            {
                Settings settings3 = new Settings(true);
                File.Delete(ApplicationSettings.ConfigLocation);
                File.WriteAllText(ApplicationSettings.ConfigLocation, JsonConvert.SerializeObject(settings3, Formatting.Indented));
                return null;
            }
            catch (Exception ex)
            {

                return ex;
            }
        }

        public static void checks(string check)
        {
            if (check == "config")
            {
                if (configCheck == "Failed")
                {
                    AnsiConsole.MarkupLine("[red]Configuration Check: \u2717[/]");
                }
                else if (configCheck == "Middle")
                {
                    AnsiConsole.MarkupLine("[yellow]Configuration Check: ![/]");
                }
                else if (configCheck == "Success")
                {
                    AnsiConsole.MarkupLine("[green]Configuration Check: \u2713[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("Configuration Check: ?");
                }
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
                    configCheck = "Success";
                }
                else
                {

                    utils.println("Settings doesn't exist, creating and setting default values.");
                    settings = new Settings(true);
                    File.WriteAllText(ApplicationSettings.ConfigLocation, JsonConvert.SerializeObject(settings, Formatting.Indented));
                    configCheck = "Success";
                }
            }

            catch (JsonReaderException ex)
            {
                utils.println("Exception Occured\n Invalid json format, do you want to reset settings?(Y/N)");
                string answer = Console.ReadLine().ToLower();

                if (answer == "y" || answer == "yes")
                {


                    Exception result = createConfig();

                    if (result != null)
                    {

                        settings = new Settings(true);
                        utils.println("Temporary settings assigned.");
                        utils.println($"Chained Exception Occured: {utils.getExceptionTy(result, settings)}");



                        configCheck = "Middle";
                    }
                    else
                    {
                        utils.println("Successfully made new config file.");
                        configCheck = "Middle";

                        settings = new Settings(true);
                    }
                }
                else // This handles "n", "no", and any other input.
                {
                    utils.println("Assuming no. Using temporary settings.");
                    configCheck = "Middle";
                    settings = new Settings(true);
                }
            }

            catch (Exception ex)
            {
                utils.println("An unknown exception occurred while loading/creating settings.");
                utils.println("Creating default settings instead.");
                utils.println($"Error details: {ex.Message}");
                configCheck = "Middle";
                settings = new Settings(true);
            }

            checks("config");
            utils.println("All checks complete!");
            Cli.main();
        }
    }
}