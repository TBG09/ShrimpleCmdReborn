using System.ComponentModel.DataAnnotations;
using ShrimpleCmd;
using ShrimpleCmd.cli;
using ShrimpleCmd.dev;
using ShrimpleCmd.utils;

namespace ShrimpleCmd
{
    public class Program
    {
        public static void Main(string[] args)
        {   // was only used for testing, so commented
            // util.println(ApplicationSettings.ascii_icon);

            ApplicationSettings.args = args;
            Startup.main();
        }
    }
}