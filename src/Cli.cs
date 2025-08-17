using ShrimpleCmd.dev;
using ShrimpleCmd.cli;
using ShrimpleCmd.utils;
using Microsoft.VisualBasic;
namespace ShrimpleCmd
{
    public class Cli
    {
        public static string input;

        public static void main(Settings settings)
        {
            while (true)
            {
                util.print($"{ApplicationSettings.CurrentDirectory}> ");
                input = util.read();
                CommandProcessor.processString(input, settings);
            }
        }
    }
}