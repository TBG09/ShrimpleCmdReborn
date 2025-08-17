using ShrimpleCmd.utils;
using ShrimpleCmd.cli;
using System.Linq;

namespace ShrimpleCmd.cli.commands
{
    public class help
    {

        public static void commandMain(string[] args)
        {

            if (args.Length > 1)
            {
                string requestedCommand = args[1];


                if (CommandProcessor.CommandDescriptions.ContainsKey(requestedCommand))
                {
                    util.println($"Help for {requestedCommand}\n{CommandProcessor.CommandDescriptions[requestedCommand]}");
                }

                else if (CommandProcessor.commands.ContainsKey(requestedCommand))
                {
                    util.println($"Help for {requestedCommand}\n{CommandProcessor.commands[requestedCommand]}");
                }

                else
                {
                    util.println($"Help not available for '{requestedCommand}'.");
                }
            }

            else
            {
                util.println("Available Commands:");
                foreach (var item in CommandProcessor.commands)
                {
                    util.println($"{item.Key} - {item.Value}");
                }
            }
        }
    }
}