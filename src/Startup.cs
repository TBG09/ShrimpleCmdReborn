using ShrimpleCmd.dev;
using ShrimpleCmd.utils;

using Newtonsoft.Json;
using Spectre.Console;
using ShrimpleCmd.log;
using Microsoft.VisualBasic;

namespace ShrimpleCmd
{
    public class Startup
    {
        private static CheckStatus configCheck;
        private static CheckStatus readOnlyCheck;

        // public enum CheckType
        // {
        //     Config,
        //     ReadOnly
        // }

        private enum CheckStatus
        {
            Failed,
            Middle,
            Success,
            IssuesSuccess
        }

        private static Exception createConfig()
        {
            try
            {
                Settings settings3 = new Settings(true, false, "!", true, true, 100, Settings.HistoryUp.UpArrow, Settings.HistoryDown.DownArrow, false);;
                File.Delete(ApplicationSettings.ConfigLocation);
                File.WriteAllText(ApplicationSettings.ConfigLocation, JsonConvert.SerializeObject(settings3, Formatting.Indented));
                return null;
            }
            catch (Exception ex)
            {

                return ex;
            }
        }

        private static void checks(string checkName, CheckStatus status)
        {
            if (status == CheckStatus.Success)
            {
                AnsiConsole.MarkupLine($"[green]{checkName} Check: \u2713[/]");
            }
            else if (status == CheckStatus.IssuesSuccess)
            {
                AnsiConsole.MarkupLine($"[yellow]{checkName}[/] [green]Check: \u2713[/]");
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
        private static void startupOperations(Settings settings)
        {
            util.println("Performing startup operations.");
            util.println("Updating Settings...");
            Settings.UpdateSettings(ApplicationSettings.ConfigLocation);
            util.println("Done!");
            util.println("Setting main settings instance...");
            ApplicationSettings.MainSettings = settings;
            util.println("Done!");
            util.println("Finished startup operations.");

        }
        
        public static void main()
        {

            
            
            Settings settings = new Settings(true, false, "!", true, true, 100, Settings.HistoryUp.UpArrow, Settings.HistoryDown.DownArrow, false);;
            Logger logger = new Logger("StartupMain", true, true);
            string settingsContent;

            util.println("Performing checks...");
            logger.info("Checking for readonly");
            try
            {
                logger.info("Checking if config exists.");
                if (File.Exists(ApplicationSettings.ConfigLocation))
                {
                    logger.info("Config exists.");
                    logger.info("Fetching file attributes of the config.");
                    FileAttributes ConfigAttribs = File.GetAttributes(ApplicationSettings.ConfigLocation);
                    logger.debug($"Attributes of {ApplicationSettings.ConfigLocation}: {ConfigAttribs}");
                    logger.info("Checking if config is read only");
                    if ((ConfigAttribs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        logger.info("Config is read only.");
                        util.print("Config file is read only\n Do you want to remove this? (Y/N)");
                        string input = util.read().ToLower();
                        if (input == "y" || input == "yes")
                        {
                            logger.info("Setting readonly attribute to false in config.");
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
                // i wonder how this could happen, eh.
                util.println("File wasn't found.");
                logger.info($"FileNotFoundException Occured: {ex}");
                readOnlyCheck = CheckStatus.Middle;

            }
            catch (Exception exc)
            {
                util.println("An unknown exception occurred while attempting to read the setitngs file's attributes");
                util.println("Creating default settings instead.");
                util.println($"Error details: {exc.Message}");
                readOnlyCheck = CheckStatus.Failed;
                logger.info($"{exc.InnerException} Occured: {exc}");
            }
            checks("Readonly", readOnlyCheck);
            logger.info("Read only checks completed.");
            logger.info("Performing config checks.");
            try
            {
                logger.info("Checking if config exists.");
                if (File.Exists(ApplicationSettings.ConfigLocation))
                {
                    logger.info("Config exists.");
                    util.println("Loading settings.");
                    settingsContent = File.ReadAllText(ApplicationSettings.ConfigLocation);
                    logger.info("Config contents feteched");
                    logger.info("Deserializing config to C# Objects.");
                    settings = JsonConvert.DeserializeObject<Settings>(settingsContent);
                    logger.info("Deserialized config into C# objects.");
                    configCheck = CheckStatus.Success;
                }
                else
                {
                    logger.info("Config not found.");
                    util.println("Settings doesn't exist, creating and setting default values.");
                    logger.info("Creating settings with default values: true,false,!,true,true,100,UpArrow,DownArrow,false");
                    settings = new Settings(true, false, "!", true, true, 100, Settings.HistoryUp.UpArrow, Settings.HistoryDown.DownArrow, false);
                    File.WriteAllText(ApplicationSettings.ConfigLocation, JsonConvert.SerializeObject(settings, Formatting.Indented));
                    logger.info("Wrote json data.");
                    configCheck = CheckStatus.Success;
                }
            }

            catch (JsonReaderException ex)
            {
                logger.info($"JsonReaderException Occurred: {ex}");
                util.println("Exception Occured\n Invalid json format, do you want to reset settings?(Y/N)");

                string answer = util.read().ToLower();

                if (answer == "y" || answer == "yes")
                {


                    Exception result = createConfig();
                    logger.info("Calling createConfig.");
                    if (result != null)
                    {
                        logger.info($"createConfig encountered an issue: {result}");
                        settings = new Settings(true, false, "!", true, true, 100, Settings.HistoryUp.UpArrow, Settings.HistoryDown.DownArrow, false);
                        logger.info("Creating settings with default values: true,false,!,true,true,100,UpArrow,DownArrow,false");
                        util.println("Using default settings instead.");
                        util.println($"Chained Exception Occured: {util.getExceptionTy(result, settings)}");



                        configCheck = CheckStatus.Middle;
                    }
                    else
                    {

                        util.println("Successfully made new config file.");
                        logger.info("Made a new config file.");
                        configCheck = CheckStatus.IssuesSuccess;

                        settings = new Settings(true, false, "!", true, true, 100, Settings.HistoryUp.UpArrow, Settings.HistoryDown.DownArrow, false);
                        logger.info("Creating settings with default values: true,false,!,true,true,100,UpArrow,DownArrow,false");
                    }
                }
                else
                {
                    util.println("Assuming no. Using defaults.");
                    configCheck = CheckStatus.Middle;
                    settings = new Settings(true, false, "!", true, true, 100, Settings.HistoryUp.UpArrow, Settings.HistoryDown.DownArrow, false);
                    logger.info("Creating settings with default values: true,false,!,true,true,100,UpArrow,DownArrow,false");
                }
            }

            catch (JsonSerializationException jsonex)
            {
                logger.info($"JsonSerializationException Occurred: {jsonex}");
                util.println("A critical error went wrong while deserializing the json to c# objects: " + jsonex.Message);
                util.println("Check the json for any mis-matched values or errors, or delete it.");
                util.println("Will use temporary settings for now");
                settings = new Settings(true, false, "!", true, true, 100, Settings.HistoryUp.UpArrow, Settings.HistoryDown.DownArrow, false);
                logger.info("Creating settings with default values: true,false,!,true,true,100,UpArrow,DownArrow,false");
            }

            catch (Exception ex)
            {
                logger.info($"{ex.InnerException} Occurred: {ex}");
                util.println("An unknown exception occurred while loading/creating settings.");
                util.println("Creating default settings instead.");
                util.println($"Error details: {ex.Message}");
                configCheck = CheckStatus.Failed;

                settings = new Settings(true, false, "!", true, true, 100, Settings.HistoryUp.UpArrow, Settings.HistoryDown.DownArrow, false); ;
                logger.info("Creating settings with default values: true,false,!,true,true,100,UpArrow,DownArrow,false");
            }

            checks("Config", configCheck);
            util.println("All checks complete!");

            logger.info("Finished configuration check");
            logger.info("Running startupOperations");
            startupOperations(settings);


            
            logger.info("Passing control to Cli.main with settings instance.");
            Cli.mainAsync(settings);
        }
    }
}