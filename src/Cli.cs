using ShrimpleCmd.dev;
using ShrimpleCmd.cli;
using ShrimpleCmd.utils;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;
using ShrimpleCmd.log;

namespace ShrimpleCmd
{
    public class Cli
    {
        public static string input;
        public static List<string> history = new List<string>();
        private static readonly object historyLock = new object();
        public static int historyIndex = 0;

        public static async Task mainAsync(Settings settings)
        {
            Logger logger = new Logger("CLI", true, true);
            int maxHistory = settings.MaxHistoryLength;
            historyIndex = history.Count; // Initialize history index to the end
            logger.info("Entering main application loop.");
            // putting logging in here possibly might slow down execution speed, so nah.
            while (true)
            {
                Console.Write($"{ApplicationSettings.CurrentDirectory}> ");
                input = ReadLineHistory(settings);

                if (!string.IsNullOrWhiteSpace(input))
                {
                    lock (historyLock)
                    {
                        if (history.Count >= maxHistory)
                        {
                            history.RemoveAt(0);
                        }
                        history.Add(input);
                    }
                    historyIndex = history.Count;

                    int commandRes = await CommandProcessor.processString(input, settings);
                    
                    if (commandRes == 1)
                    {
                        string unicodeConvertedInput = util.ConvertToSymbols(input);
                        int sysres = SystemCommandHandler.SystemCommand(unicodeConvertedInput);
                        
                        // to avoid some random program returning 1 and it interpretting that as an invalid command.
                        if (sysres == 15382395) 
                        {
                            
                            util.println($"Unrecognized command '{input}'.");
                        }
                    }
                }
            }
        }

        public static string ReadLineHistory(Settings settings)
        {
            Logger logger = new Logger("KeyHandler", true, true);
            string currentInput = "";
            int cursorPosition = 0;

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return currentInput;
                }
                else if (keyInfo.Key == (ConsoleKey)settings.HistoryUpKey)
                {
                    logger.info("HistoryUp key pressed");
                    if (history.Count > 0 && historyIndex > 0)
                    {
                        historyIndex--;
                        currentInput = history[historyIndex];
                        UpdateLine(currentInput);
                        cursorPosition = currentInput.Length;
                    }
                }
                else if (keyInfo.Key == (ConsoleKey)settings.HistoryDownKey)
                {
                    logger.info("HistoryDown key pressed");
                    if (historyIndex < history.Count - 1)
                    {
                        historyIndex++;
                        currentInput = history[historyIndex];
                        UpdateLine(currentInput);
                        cursorPosition = currentInput.Length;
                    }
                    else if (historyIndex == history.Count - 1)
                    {
                        historyIndex = history.Count;
                        currentInput = "";
                        UpdateLine(currentInput);
                        cursorPosition = 0;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && currentInput.Length > 0 && cursorPosition > 0)
                {
                    currentInput = currentInput.Remove(cursorPosition - 1, 1);
                    cursorPosition--;
                    UpdateLine(currentInput);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    currentInput = currentInput.Insert(cursorPosition, keyInfo.KeyChar.ToString());
                    cursorPosition++;
                    UpdateLine(currentInput);
                }
            }
        }

        private static void UpdateLine(string text)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"{ApplicationSettings.CurrentDirectory}> {text}");
        }
    }
}
