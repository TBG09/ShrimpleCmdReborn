using System.Diagnostics;
using System.IO;
using ShrimpleCmd.dev;
using ShrimpleCmd.log;
using ShrimpleCmd.utils;
using System.Collections.Generic;
using System.Linq;
using System.IO.Pipelines;

namespace ShrimpleCmd.cli
{
    public class SystemCommandHandler
    {
        public static int SystemCommand(string command)
        {
            string commandName = command.Split(' ')[0];
            HashSet<string> uniqueExecutableFiles = new HashSet<string>();

            string PathVar = Environment.GetEnvironmentVariable("Path");
            string[] folders = PathVar?.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

            foreach (var item in folders)
            {
                string fullPath = Path.Combine(item, commandName);
                if (File.Exists(fullPath + ".exe")) uniqueExecutableFiles.Add(fullPath + ".exe");
                if (File.Exists(fullPath + ".bat")) uniqueExecutableFiles.Add(fullPath + ".bat");
                if (File.Exists(fullPath + ".cmd")) uniqueExecutableFiles.Add(fullPath + ".cmd");
            }

            string currentDirFullPath = Path.Combine(ApplicationSettings.CurrentDirectory, commandName);
            if (File.Exists(currentDirFullPath + ".exe")) uniqueExecutableFiles.Add(currentDirFullPath + ".exe");
            if (File.Exists(currentDirFullPath + ".bat")) uniqueExecutableFiles.Add(currentDirFullPath + ".bat");
            if (File.Exists(currentDirFullPath + ".cmd")) uniqueExecutableFiles.Add(currentDirFullPath + ".cmd");

            List<string> executableFilesFound = uniqueExecutableFiles.ToList();

            int resul = Executor(executableFilesFound, command);
            return resul;
        }

        public static int Executor(List<string> files, string command)
        {
            Logger logger = new Logger("ProcessExecutor", true, true);
            if (files.Count == 0)
            {
                util.println("Command not found in path or current directory, check if the executable your looking for exists.");
                return 15382395;
            }

            int spaceIndex = command.IndexOf(' ');
            string strippedCommand = (spaceIndex != -1) ? command.Substring(spaceIndex + 1) : string.Empty;

            string fileToExecute = string.Empty;

            if (files.Count > 1)
            {
                util.println("There is more than one kind of executable for the command you typed. Choose one. Type exit to, well, exit.");
                for (int i = 0; i < files.Count; i++)
                {
                    util.println($"{i}. {files[i]}");
                }
                string userChoice = util.read();
                if (userChoice == "exit")
                {
                    return 0;
                }
                while (!ExCheck(userChoice, files))
                {
                    if (userChoice == "exit")
                    {
                        return 0;
                    }
                    userChoice = util.read();
                }
                fileToExecute = files[int.Parse(userChoice)];
            }
            else
            {
                fileToExecute = files[0];
            }
            logger.info("Creating new ProcessInfo");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            logger.info("Setting file name to " + fileToExecute);
            startInfo.FileName = fileToExecute;
            logger.info("Setting arguments to " + strippedCommand);
            startInfo.Arguments = strippedCommand;
            logger.info("Setting Redirection of stdout and stderr.");
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            logger.info("Finalizing settings.");
            startInfo.CreateNoWindow = true;

            try
            {
            using (Process process = new Process())
            {
                logger.info("Setting output listeners for stdout and stderr.");
                process.StartInfo = startInfo;
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        {
                            Console.WriteLine(e.Data);
                        }
                    };

                    logger.info("Starting process");
                    process.Start();
                    logger.info("Process started without issue.");
                    logger.info($"Process ID: {process.Id}");

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();
                    logger.info("Process has exited.");
                    logger.info("Process exit code: " + process.ExitCode);
                    return 0;
                    }
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    
                    logger.error($"Failed to start process. Check the file path. Error: {ex}");
                    return -1;
                }
                catch (InvalidOperationException ex)
                {
                    
                    logger.error($"Failed to start process due to misconfiguration. Error: {ex}");
                    return -1;
                }
                catch (Exception ex)
                {
                    logger.error($"An unexpected error occurred while running the process. Error: {ex}");
                    return -1;
                }
        }

        private static bool ExCheck(string str, List<string> list)
        {
            Logger logger = new Logger("ExCheck", true, true);
            if (string.IsNullOrWhiteSpace(str))
            {
                util.println("Provide a valid number.");
                return false;
            }
            try
            {
                int index = int.Parse(str);
                if (index < 0 || index >= list.Count)
                {
                    util.println("Invalid number. Please choose a number from the list.");
                    return false;
                }
            }
            catch (OverflowException exo)
            {
                logger.error($"OverflowException Occurred: {exo.Message}");
                util.println($"Provided integer overflows {int.MaxValue}");
                return false;
            }
            catch (FormatException exf)
            {
                logger.error($"FormatException Occurred: {exf.Message}");
                util.println($"Received something else other than an Int.");
                return false;
            }
            return true;
        }
    }
}