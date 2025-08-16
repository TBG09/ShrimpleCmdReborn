using ShrimpleCmd.dev;
using ShrimpleCmd.cli;
using Microsoft.VisualBasic;
namespace ShrimpleCmd
{
    public class Cli
    {
        public static string input;

        public static void main()
        {
            while (true)
            {
                utils.print($"{ApplicationSettings.CurrentDirectory}> ");
                input = utils.read();
                CommandProcessor.processString(input);
            }
        }
    }
}