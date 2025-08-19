using System.Reflection.Metadata.Ecma335;
using ShrimpleCmd.dev;
using System.Collections.ObjectModel;
using ShrimpleCmd.utils;
using ShrimpleCmd.cli.commands;
using ShrimpleCmd.log;
using System.Threading.Tasks;

namespace ShrimpleCmd.cli
{
    public class CommandProcessor
    {
        static Settings settings;

        private static void EchoCommand(string[] args)
        {
            util.println(string.Join(" ", args.Skip(1)));
        }

        private static void ExitCommand(string[] args)
        {
            Logger logger = new Logger("ExitCommand", true, true);
            try
            {
                if (args.Length < 2)
                {
                    logger.info("Exitting with code 0");
                    Environment.Exit(0);
                }
                else
                {
                    logger.info("Exitting with code " + args[1]);
                    Environment.Exit(int.Parse(args[1]));
                }
            }
            catch (OverflowException exo)
            {
                logger.error($"OverflowException Occureed: {exo}");
                util.println($"Provided integer overflows {int.MaxValue}");

            }
            catch (FormatException exf)
            {
                logger.error($"FormatException Occurred: {exf}");
                util.println($"Received something else other than an Int.");
            }
        }

        public static void ClearCommand(string[] args)
        {
            Console.Clear();
        }

        public static Dictionary<string, string> commands = new Dictionary<string, string>()
        {
            {"echo", "Prints text to the console."},
            {"exit", "Exits the application."},
            {"cls/clear", "Clears the console."},
            {"shrimple", "The main manager command for shrimple."}
        };

        public static Dictionary<string, string> CommandDescriptions = new Dictionary<string, string>()
        {
            {"echo", "Takes 1 or more arguments.\nTakes a group of arguments which is what will be its output."},
            {"help", "Takes 1 optional argument\n Outputs all available commands in a short description.\n Providing a command as an argument will give detailed info on that command."},
            {"exit", "Takes 1 optional argument\n Exits the process.\n if an argument is added, it will expect it as an int and exit the process with that code instead of the default 0."},
            {"cls/clear", "Takes 0 arguments\n Clears the screen(spoiler: not really, it just moves everything up.)"},
            {"shrimple", "Handles application-level commands for versioning, updating, and configuration.\n\n**Subcommands:**\n- `version`: Displays the installed version and checks for the latest available version online.\n- `update [--prerelease] [version]`: Downloads and installs the latest stable version by default. The `--prerelease` flag allows updating to a pre-release, and a specific `[version]` can be provided to install it.\n- `config <action> [name] [value]`: Manages application settings. `get` retrieves a setting's value, `set` changes it, `toggle` inverts a boolean, `reset` restores the default, and `list` shows all settings or a specific group." },
        };

        private static readonly Dictionary<string, Func<string[], Task>> commandActions = new Dictionary<string, Func<string[], Task>>()
        {
            {"echo", args => { EchoCommand(args); return Task.CompletedTask; } },
            {"exit", args => { ExitCommand(args); return Task.CompletedTask; } },
            {"help", args => { help.commandMain(args); return Task.CompletedTask; } },
            {"clear", args => { ClearCommand(args); return Task.CompletedTask; } },
            {"cls", args => { ClearCommand(args); return Task.CompletedTask; } },
            {"shrimple", shrimple.commandMain }
        };

        public static async Task<int> processString(string command, Settings settings)
        {
            Logger logger = new Logger("CommandProcessor", true, true);
            if (string.IsNullOrWhiteSpace(command))
            {
                return 1;
            }
            
            logger.info($"Processing command: '{command}'");
            

            string lowerCommand = command.ToLower();
            
            logger.info("Splitting command");
            string[] commandParts = lowerCommand.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (commandParts.Length == 0)
            {
                return 1;
            }

            string commandName;
            logger.info("Checking if prefix is enforced.");
            if (settings.enforcePrefix)
            {
                logger.info("Checking if command doesn't start with prefix.");
                if (!commandParts[0].StartsWith(settings.internalCommandPrefix.ToLower()))
                {
                    logger.info($"Command '{commandParts[0]}' doesn't start with required prefix '{settings.internalCommandPrefix}'");
                    return 1;
                }
                commandName = commandParts[0].Substring(settings.internalCommandPrefix.Length);
            }
            else
            {
                commandName = commandParts[0].TrimStart(settings.internalCommandPrefix.ToLower().ToCharArray());
            }

            logger.info($"Extracted command name: '{commandName}'");

            if (commandActions.ContainsKey(commandName))
            {
                logger.info($"Running command '{commandName}' with {commandParts.Length} args");
                
                // Create args array
                string[] originalArgs = command.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                string[] argsToPass;
                
                if (settings.enforcePrefix)
                {
                    argsToPass = new string[originalArgs.Length];
                    argsToPass[0] = commandName; 
                    Array.Copy(originalArgs, 1, argsToPass, 1, originalArgs.Length - 1);
                }
                else
                {
                    argsToPass = originalArgs;
                    argsToPass[0] = commandName;
                }
                
                await commandActions[commandName](argsToPass);
                return 0;
            }
            else
            {
                logger.info($"Command '{commandName}' not found in commandActions");
                logger.info($"Available commands: {string.Join(", ", commandActions.Keys)}");
                return 1;
            }
        }
    }
}