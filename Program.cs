using System.ComponentModel.DataAnnotations;
using ShrimpleCmd;
using ShrimpleCmd.dev;

namespace ShrimpleCmd
{
    public class Program
    {
        public static void Main(string[] args)
        {
            utils.println(ApplicationSettings.ascii_icon);
            ShrimpleCmd.dev.ApplicationSettings.args = args;
            ShrimpleCmd.Startup.main();
        }
    }
}