using System.Runtime.CompilerServices;
using System.IO;
using Spectre.Console;
using ShrimpleCmd.dev;

namespace ShrimpleCmd.log
{
    public class Logger
    {
        public string name { get; set; }
        public bool showClassName { get; set; }
        public bool ColorEnabled { get; set; }

        public Logger(string nameS, bool showClassN, bool Color)
        {
            this.name = nameS;
            this.showClassName = showClassN;
            this.ColorEnabled = Color;
        }

        private void WriteLog(string level, string color, string message, string callerFile)
        {
            string className = Path.GetFileNameWithoutExtension(callerFile);
            string displayTag = showClassName ? $"{className} - {this.name}" : this.name;

            if (ApplicationSettings.MainSettings.ShowLoggingOutput)
            {
                if (ColorEnabled)
                {
                    // [[LEVEL]]
                    AnsiConsole.Write(new Markup($"[grey]{Markup.Escape("[[")}[/]"));
                    AnsiConsole.Write(new Markup($"[{color}]{level}[/]"));
                    AnsiConsole.Write(new Markup($"[grey]{Markup.Escape("]]")}[/] "));

                    // [[ClassName - LoggerName]]
                    AnsiConsole.Write(new Markup($"[grey]{Markup.Escape("[[")}[/]"));
                    AnsiConsole.Write(new Markup($"[lime]{Markup.Escape(displayTag)}[/]"));
                    AnsiConsole.Write(new Markup($"[grey]{Markup.Escape("]]")}[/] "));

                    // Message
                    AnsiConsole.WriteLine(Markup.Escape(message));
                }
                else
                {
                    AnsiConsole.WriteLine($"[{level}] [{displayTag}] {message}");
                }
            }

            // File log gets timestamp
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            LogManager.WriteToFile($"[{timestamp}] [{level}]", displayTag, message);
        }

        public void info(string message, [CallerFilePath] string callerFile = "") =>
            WriteLog("INFO", "blue", message, callerFile);

        public void debug(string message, [CallerFilePath] string callerFile = "") =>
            WriteLog("DEBUG", "lightskyblue1", message, callerFile);

        public void warn(string message, [CallerFilePath] string callerFile = "") =>
            WriteLog("WARN", "yellow", message, callerFile);

        public void error(string message, [CallerFilePath] string callerFile = "") =>
            WriteLog("ERROR", "red", message, callerFile);

        public void fatal(string message, [CallerFilePath] string callerFile = "")
        {
            string className = Path.GetFileNameWithoutExtension(callerFile);
            string displayTag = showClassName ? $"{className} - {this.name}" : this.name;

            if (ApplicationSettings.MainSettings.ShowLoggingOutput)
            {
                if (ColorEnabled)
                {
                    AnsiConsole.Write(new Markup("[bold red]FATAL:[/] "));
                    AnsiConsole.Write(new Markup($"[grey]{Markup.Escape("[[")}[/]"));
                    AnsiConsole.Write(new Markup($"[lime]{Markup.Escape(displayTag)}[/]"));
                    AnsiConsole.Write(new Markup($"[grey]{Markup.Escape("]]")}[/] "));
                    AnsiConsole.WriteLine(Markup.Escape(message));
                }
                else
                {
                    AnsiConsole.WriteLine($"[FATAL] [{displayTag}] {message}");
                }
            }

            // File log with timestamp
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            LogManager.WriteToFile($"[{timestamp}] [FATAL]", displayTag, message);
        }
    }
}
