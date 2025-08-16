using System.Reflection.Metadata.Ecma335;
using ShrimpleCmd.dev;

namespace ShrimpleCmd.cli
{
    public class CommandProcessor
    {
        public static void processString(string command)
        {
            string[] commandParts = command.Split(" ");

            if (commandParts[0] == "echo")
            {
                utils.println(string.Join(" ", commandParts.Skip(1)));
                return;
            }
            else if (commandParts[0] == "version")
            {
                utils.println(ApplicationSettings.ascii_icon);
                utils.println($"ShrimpleCmd (Reborn)\nVersion: 0.0.1 Alpha\nBuild {ApplicationSettings.Build}");
            }
            else if (commandParts[0] == "exit")
            {
                Environment.Exit(0);
            }
            else
            {
                utils.println("Unknown command");
                return;
            }
        }
    }
}