using ShrimpleCmd.dev;
using ShrimpleCmd.utils;

using Newtonsoft.Json;
using Spectre.Console;

namespace ShrimpleCmd
{
    public class Startup
    {
        public static CheckStatus configCheck;
        public static CheckStatus readOnlyCheck;

        // public enum CheckType
        // {
        //     Config,
        //     ReadOnly
        // }

        public enum CheckStatus
        {
            Failed,
            Middle,
            Success
        }

        public static Exception createConfig()
        {
            try
            {
                Settings settings3 = new Settings(true, false, "!", true);
                File.Delete(ApplicationSettings.ConfigLocation);
                File.WriteAllText(ApplicationSettings.ConfigLocation, JsonConvert.SerializeObject(settings3, Formatting.Indented));
                return null;
            }
            catch (Exception ex)
            {

                return ex;
            }
        }

        public static void checks(string checkName, CheckStatus status)
        {
            if (status == CheckStatus.Success)
            {
                AnsiConsole.MarkupLine($"[green]{checkName} Check: \u2713[/]");
            }
            else if (status == CheckStatus.Middle)
            {
                AnsiConsole.MarkupLine($"[yellow]{checkName} Check: ![/]");
            }
            else if (status == CheckStatus.Failed)
            {
                AnsiConsole.MarkupLine($"[red]{checkName} Check: \u2717[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"{checkName} Check: ?");
            }
        }

        public static void main()
        {

            Settings settings = new Settings(true, false, "!", true);
            string settingsContent;

            util.println("Performing checks...");
            try
            {
                if (File.Exists(ApplicationSettings.ConfigLocation))
                {
                    FileAttributes ConfigAttribs = File.GetAttributes(ApplicationSettings.ConfigLocation);
                    if ((ConfigAttribs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        util.print("Config file is read only\n Do you want to remove this? (Y/N)");
                        string input = util.read().ToLower();
                        if (input == "y" || input == "yes")
                        {
                            // https://stackoverflow.com/questions/8081242/c-sharp-make-file-read-write-from-readonly
                            var attr = File.GetAttributes(ApplicationSettings.ConfigLocation);
                            attr = attr & ~FileAttributes.ReadOnly;
                            File.SetAttributes(ApplicationSettings.ConfigLocation, attr);
                            readOnlyCheck = CheckStatus.Success;
                        }
                        else
                        {
                            util.println("Assuming No.\nWill not remove read only, this will cause an exception later on if the config is attempted to be changed, which should be handled.");
                            readOnlyCheck = CheckStatus.Middle;
                        }

                    }
                    else
                    {
                        readOnlyCheck = CheckStatus.Success;
                    }
                }
                else
                {
                    readOnlyCheck = CheckStatus.Success;
                }

            }

            catch (FileNotFoundException ex)
            {
                util.println("File wasn't found.");
                if (settings.debugMode)
                {
                    util.println("Debug details: " + ex.ToString());
                }
                readOnlyCheck = CheckStatus.Middle;

            }
            catch (Exception exc)
            {
                util.println("An unknown exception occurred while attempting to read the setitngs file's attributes");
                util.println("Creating default settings instead.");
                util.println($"Error details: {exc.Message}");
                readOnlyCheck = CheckStatus.Failed;
                if (settings.debugMode)
                {
                    util.println("Debug details: " + exc.ToString());
                }
            }
            checks("Readonly", readOnlyCheck);
            try
            {

                if (File.Exists(ApplicationSettings.ConfigLocation))
                {
                    util.println("Loading settings.");
                    settingsContent = File.ReadAllText(ApplicationSettings.ConfigLocation);
                    settings = JsonConvert.DeserializeObject<Settings>(settingsContent);
                    configCheck = CheckStatus.Success;
                }
                else
                {

                    util.println("Settings doesn't exist, creating and setting default values.");
                    settings = new Settings(true, false, "!", true);
                    File.WriteAllText(ApplicationSettings.ConfigLocation, JsonConvert.SerializeObject(settings, Formatting.Indented));
                    configCheck = CheckStatus.Success;
                }
            }

            catch (JsonReaderException ex)
            {
                util.println("Exception Occured\n Invalid json format, do you want to reset settings?(Y/N)");
                if (settings.debugMode)
                {
                    util.println(ex.ToString());
                }
                string answer = Console.ReadLine().ToLower();

                if (answer == "y" || answer == "yes")
                {


                    Exception result = createConfig();

                    if (result != null)
                    {

                        settings = new Settings(true, false, "!", true);
                        util.println("Using default settings instead.");
                        util.println($"Chained Exception Occured: {util.getExceptionTy(result, settings)}");



                        configCheck = CheckStatus.Middle;
                    }
                    else
                    {
                        util.println("Successfully made new config file.");
                        configCheck = CheckStatus.Middle;

                        settings = new Settings(true, false, "!", true);
                    }
                }
                else
                {
                    util.println("Assuming no. Using defaults.");
                    configCheck = CheckStatus.Middle;
                    settings = new Settings(true, false, "!", true);
                }
            }

            catch (Exception ex)
            {
                util.println("An unknown exception occurred while loading/creating settings.");
                util.println("Creating default settings instead.");
                util.println($"Error details: {ex.Message}");
                configCheck = CheckStatus.Failed;
                if (settings.debugMode)
                {
                    util.println("Debug details: " + ex.ToString());
                }
                settings = new Settings(true, false, "!", true);

            }

            checks("Config", configCheck);
            util.println("All checks complete!");
            



            Cli.main(settings);
        }
    }
}