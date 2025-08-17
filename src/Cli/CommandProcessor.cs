using System.Reflection.Metadata.Ecma335;
using ShrimpleCmd.dev;
using System.Collections.ObjectModel;
using ShrimpleCmd.utils;
using ShrimpleCmd.cli.commands;
namespace ShrimpleCmd.cli
{
    public class CommandProcessor
    {

        static Settings settings;
        private static void EchoCommand(string[] args)
        {
            util.println(string.Join(" ", args.Skip(1)));
        }

        private static void VersionCommand(string[] args)
        {
            util.println(ApplicationSettings.ascii_icon);
            util.println($"ShrimpleCmd (Reborn)\nVersion: {ApplicationSettings.Version}\nBuild {ApplicationSettings.Build}");
            util.println("Changes/Additions:\n Added commands version and exit.\n Fixed a few bugs.\n Added a read only configuration file check.\n Also added a boolean to config to control whether a prefix is needed or not.\n Technical Stuff:\n everything moved into a src\ folder\n changed the command processor to be more advanced than the last.");
        }

        private static void ExitCommand(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Environment.Exit(0);
                }
                else
                {
                    Environment.Exit(int.Parse(args[1]));
                }
            }
            catch (OverflowException exo)
            {
                util.println($"Provided integer overflows {int.MaxValue}");
                if (settings.debugMode)
                {
                    util.println($"Debug info: {exo}");
                }
            }
            catch (FormatException exf)
            {
                util.println($"Received something else other than an Int.");
                if (settings.debugMode)
                {
                    util.println($"Debug info: {exf}");
                }
            }
        }

        public static Dictionary<string, string> commands = new Dictionary<string, string>()
        {
            {"echo", "Prints text to the console."},
            {"version", "Displays the application version."},
            {"exit", "Exits the application."}
        };

        public static Dictionary<string, string> CommandDescriptions = new Dictionary<string, string>()
        {
            {"echo", "Takes 1 or more arguments.\nTakes a group of arguments which is what will be its output."},
            {"version", "Takes 0 arguments\n Outputs the version splash, and info about the current version."},
            {"help", "Takes 1 optional argument\n Outputs all available commands in a short description.\n Providing a command as an argument will give detailed info on that command."},
            {"exit", "Takes 1 optional argument\n Exits the process.\n if an argument is added, it will expect it as an int and exit the process with that code instead of the default 0."}
        };


        
        private static readonly Dictionary<string, Action<string[]>> commandActions = new Dictionary<string, Action<string[]>>()
        {
            {"echo", EchoCommand},
            {"version", VersionCommand},
            {"exit", ExitCommand},
            {"help", help.commandMain}
        };

        public static void processString(string command, Settings settings)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return;
            }
            
            string[] commandParts = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (commandParts.Length == 0)
            {
                return;
            }

            string commandName;

            if (settings.enforcePrefix)
            {
                if (!commandParts[0].StartsWith(settings.internalCommandPrefix))
                {
                    util.println($"Unrecognized command '{commandParts[0]}'.");
                    return;
                }
                commandName = commandParts[0].Substring(settings.internalCommandPrefix.Length);
            }
            else
            {
                commandName = commandParts[0].TrimStart(settings.internalCommandPrefix.ToCharArray());
            }

            if (commandActions.ContainsKey(commandName))
            {
                commandActions[commandName](commandParts);
            }
            else
            {
                string displayCommand = settings.enforcePrefix 
                    ? settings.internalCommandPrefix + commandName 
                    : commandParts[0];
                util.println($"Unrecognized command '{displayCommand}'.");
            }
        }
    }
}