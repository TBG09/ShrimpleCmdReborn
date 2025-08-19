using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;

// Assumed namespaces based on the user's provided code.
// You may need to adjust these depending on your project's structure.
using Newtonsoft.Json;
using ShrimpleCmd.dev;
using ShrimpleCmd.log;
using ShrimpleCmd.utils;

namespace ShrimpleCmd.cli.commands
{
    public class shrimple
    {
        // HttpClient instance for making web requests.
        private static readonly HttpClient _httpClient = new HttpClient();
        // The GitHub API URL for fetching release information.
        private const string GitHubApiUrl = "https://api.github.com/repos/TBG09/ShrimpleCmdReborn/releases";

        public static async Task commandMain(string[] args)
        {
            // Skip the first argument if it's "shrimple" (the command name itself)
            string[] actualArgs;
            if (args.Length > 0 && args[0].ToLower() == "shrimple")
            {
                // Remove the "shrimple" command name and work with the subcommands
                actualArgs = args.Skip(1).ToArray();
            }
            else
            {
                actualArgs = args;
            }

            if (actualArgs.Length == 0)
            {
                util.println("Usage: shrimple [command] [--prerelease] [version]");
                util.println("Commands:");
                util.println("  version - Show version information");
                util.println("  update [--prerelease] [version] - Update ShrimpleCmd");
                util.println("  config <action> [name] [value] - Manage configuration");
                util.println("Be careful here as this isn't tested.");
                return;
            }

            _httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ShrimpleCmd", "1.0"));

            switch (actualArgs[0].ToLower())
            {
                case "version":
                    await HandleVersionCommand();
                    break;
                case "update":
                    await HandleUpdateCommand(actualArgs);
                    break;
                case "config":
                    HandleConfigCommand(actualArgs);
                    break;
                default:
                    util.println($"Unknown subcommand: {actualArgs[0]}");
                    util.println("Available subcommands: version, update, config");
                    break;
            }
        }

        private static async Task HandleVersionCommand()
        {
            try
            {
                util.println(ApplicationSettings.ascii_icon);
                util.println($"ShrimpleCmd (Reborn)\nVersion: {ApplicationSettings.Version}\nBuild {ApplicationSettings.Build}");
                util.println(vars.versionChange);

                var releases = await GetReleasesFromGitHub();
                
                if (releases.ValueKind != JsonValueKind.Array || !releases.EnumerateArray().Any())
                {
                    util.println("Latest version: Could not retrieve latest version from GitHub.");
                    return;
                }

                var latestRelease = releases.EnumerateArray().FirstOrDefault(r => !r.GetProperty("prerelease").GetBoolean());
                string latestVersion = latestRelease.ValueKind != JsonValueKind.Undefined
                    ? latestRelease.GetProperty("tag_name").GetString()
                    : "N/A";

                util.println($"Latest version available: {latestVersion}");
            }
            catch (Exception ex)
            {
                util.println($"An error occurred while checking versions: {ex.Message}");
            }
        }

        private static async Task HandleUpdateCommand(string[] args)
        {
            try
            {
                bool includePrerelease = args.Contains("--prerelease");
                string targetVersion = args.FirstOrDefault(arg => !arg.StartsWith("--"));

                var releases = await GetReleasesFromGitHub();
                
                if (releases.ValueKind != JsonValueKind.Array || !releases.EnumerateArray().Any())
                {
                    util.println("No proper releases found on GitHub.");
                    return;
                }

                JsonElement selectedRelease;
                if (!string.IsNullOrEmpty(targetVersion))
                {
                    // The fix: Use EnumerateArray() before calling FirstOrDefault
                    selectedRelease = releases.EnumerateArray().FirstOrDefault(r => r.GetProperty("tag_name").GetString().Equals(targetVersion, StringComparison.OrdinalIgnoreCase));
                    if (selectedRelease.ValueKind == JsonValueKind.Undefined)
                    {
                        util.println($"No release found with version '{targetVersion}'.");
                        return;
                    }
                }
                else if (includePrerelease)
                {

                    selectedRelease = releases.EnumerateArray().FirstOrDefault(r => r.GetProperty("prerelease").GetBoolean());
                    if (selectedRelease.ValueKind == JsonValueKind.Undefined)
                    {
                        util.println("No prerelease found.");
                        return;
                    }
                }
                else
                {

                    selectedRelease = releases.EnumerateArray().FirstOrDefault(r => !r.GetProperty("prerelease").GetBoolean());
                    if (selectedRelease.ValueKind == JsonValueKind.Undefined)
                    {
                        util.println("No proper release found.");
                        return;
                    }
                }

                util.println($"Found new version: {selectedRelease.GetProperty("tag_name").GetString()}");

                string architectureString = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "x64",
                    Architecture.X86 => "x86",
                    _ => throw new PlatformNotSupportedException("Unsupported architecture.")
                };

                var asset = selectedRelease.GetProperty("assets").EnumerateArray()
                    .FirstOrDefault(a => a.GetProperty("name").GetString().Contains(architectureString, StringComparison.OrdinalIgnoreCase));

                if (asset.ValueKind == JsonValueKind.Undefined)
                {
                    util.println($"No update package found for {architectureString} architecture.");
                    return;
                }

                string downloadUrl = asset.GetProperty("browser_download_url").GetString();
                string tempDir = Path.Combine(Path.GetTempPath(), "ShrimpleCmd_Update");
                string tempZipPath = Path.Combine(tempDir, "update.zip");

                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
                Directory.CreateDirectory(tempDir);

                util.println("Downloading update...");
                await DownloadFileAsync(downloadUrl, tempZipPath);

                util.println("Extracting files...");
                ZipFile.ExtractToDirectory(tempZipPath, tempDir);

                util.println("Update staged. Starting update process...");

                string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                string scriptPath = Path.Combine(currentDir, "update_script.bat");

                string scriptContent = $@"
@echo off
echo Waiting for ShrimpleCmd to close...
timeout /t 5 >nul
echo Copying new files...
xcopy ""{tempDir}"" ""{currentDir}"" /s /e /y /h /k
echo Cleaning up...
rmdir ""{tempDir}"" /s /q
echo Update complete. Relaunching...
start ""{Process.GetCurrentProcess().ProcessName}"" ""{Assembly.GetEntryAssembly()?.Location}""
exit
";
                File.WriteAllText(scriptPath, scriptContent);

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = scriptPath,
                    Arguments = "",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(startInfo);

                Environment.Exit(0);
            }
            catch (PlatformNotSupportedException pex)
            {
                util.println(pex.Message);
            }
            catch (Exception ex)
            {
                util.println($"An error occurred during update: {ex.Message}");
            }
        }

        // This method handles the 'config' command.
        public static void HandleConfigCommand(string[] args)
        {
            Logger logger = new Logger("ConfigCommand", true, true);
            if (args.Length < 2)
            {
                util.println("Expected an action. Try `!help config` for more info.");
                return;
            }

            // Enum for different config actions.
            if (!Enum.TryParse(args[1], true, out actions action))
            {
                util.println($"Invalid action: '{args[1]}'.");
                return;
            }

            string settingsName = (args.Length > 2) ? args[2] : null;
            string settingsValue = (args.Length > 3) ? args[3] : null;

            switch (action)
            {
                case actions.get:
                    logger.info("get action detected.");
                    if (settingsName == null)
                    {
                        util.println("Error: 'get' requires a setting name.");
                        return;
                    }
                    if (!ApplicationSettings.MainSettings.ConfigStuff.ContainsKey(settingsName))
                    {
                        util.println($"Error: Setting '{settingsName}' not found.");
                        return;
                    }
                    util.println(ApplicationSettings.MainSettings.ConfigStuff[settingsName]?.ToString());
                    break;

                case actions.set:
                    if (settingsName == null || settingsValue == null)
                    {
                        util.println("Error: 'set' requires a setting name and a value. Usage: `!config set <name> <value>`.");
                        return;
                    }
                    try
                    {
                        if (!ApplicationSettings.MainSettings.ConfigStuff.ContainsKey(settingsName))
                        {
                            util.println($"Error: Setting '{settingsName}' not found.");
                            return;
                        }
                        logger.info("Fetching propety name for '" + settingsName + "'");
                        // Use reflection to find the actual property name
                        string propertyName = GetPropertyNameFromKey(settingsName);
                        if (propertyName == null)
                        {
                            util.println($"Error: '{settingsName}' does not map to a valid property.");
                            return;
                        }
                        logger.info($"Fetching the property and setting value to {settingsValue}");
                        // Get the property and set its value
                        PropertyInfo propertyInfo = ApplicationSettings.MainSettings.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                        if (propertyInfo == null)
                        {
                            util.println($"Error: Property '{propertyName}' not found on Settings object.");
                            return;
                        }

                        object convertedValue;
                        if (propertyInfo.PropertyType.IsEnum)
                        {
                            convertedValue = Enum.Parse(propertyInfo.PropertyType, settingsValue, true);
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(settingsValue, propertyInfo.PropertyType);
                        }

                        propertyInfo.SetValue(ApplicationSettings.MainSettings, convertedValue);

                        // Update ConfigStuff after setting the real property
                        ApplicationSettings.MainSettings.ConfigStuff[settingsName] = convertedValue;

                        string updatedSettingsContent = JsonConvert.SerializeObject(ApplicationSettings.MainSettings, Formatting.Indented);
                        File.WriteAllText(ApplicationSettings.ConfigLocation, updatedSettingsContent);
                        util.println($"Set '{settingsName}' to '{settingsValue}'");
                    }
                    catch (Exception ex)
                    {
                        logger.info($"{ex.InnerException} Occurred: {ex}");
                        util.println($"Error: Could not set '{settingsName}' to '{settingsValue}'.");
                        util.println($"Details: {ex.Message}");
                    }
                    break;

                case actions.toggle:
                    if (settingsName == null)
                    {
                        util.println("Error: 'toggle' requires a setting name.");
                        return;
                    }
                    if (!ApplicationSettings.MainSettings.ConfigStuff.ContainsKey(settingsName))
                    {
                        util.println($"Error: Setting '{settingsName}' not found.");
                        return;
                    }
                    try
                    {
                        string propertyName = GetPropertyNameFromKey(settingsName);
                        if (propertyName == null)
                        {
                            util.println($"Error: '{settingsName}' does not map to a valid property.");
                            return;
                        }

                        PropertyInfo propertyInfo = ApplicationSettings.MainSettings.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

                        if (propertyInfo.PropertyType != typeof(bool))
                        {
                            util.println($"Error: '{settingsName}' is not a boolean setting and cannot be toggled.");
                            return;
                        }

                        bool currentValue = (bool)propertyInfo.GetValue(ApplicationSettings.MainSettings);
                        bool newValue = !currentValue;
                        propertyInfo.SetValue(ApplicationSettings.MainSettings, newValue);

                        // Update ConfigStuff
                        ApplicationSettings.MainSettings.ConfigStuff[settingsName] = newValue;

                        string updatedSettingsContent = JsonConvert.SerializeObject(ApplicationSettings.MainSettings, Formatting.Indented);
                        File.WriteAllText(ApplicationSettings.ConfigLocation, updatedSettingsContent);
                        util.println($"Setting '{settingsName}' toggled to '{newValue}'.");
                    }
                    catch (Exception ex)
                    {
                        logger.info($"{ex.InnerException} Occurred: {ex}");
                        util.println($"Error: Could not toggle '{settingsName}'.");
                        util.println($"Details: {ex.Message}");
                    }
                    break;

                case actions.reset:
                    if (settingsName == null)
                    {
                        util.println("Error: 'reset' requires a setting name.");
                        return;
                    }
                    if (!ApplicationSettings.MainSettings.ConfigStuff.ContainsKey(settingsName))
                    {
                        util.println($"Error: Setting '{settingsName}' not found.");
                        return;
                    }

                    try
                    {
                        string propertyName = GetPropertyNameFromKey(settingsName);
                        if (propertyName == null)
                        {
                            util.println($"Error: '{settingsName}' does not map to a valid property.");
                            return;
                        }

                        PropertyInfo propertyInfo = ApplicationSettings.MainSettings.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                        if (propertyInfo == null)
                        {
                            util.println($"Error: Property '{propertyName}' not found on Settings object.");
                            return;
                        }

                        object resetValue = GetDefaultValue(settingsName);
                        if (resetValue != null)
                        {
                            propertyInfo.SetValue(ApplicationSettings.MainSettings, resetValue);
                            ApplicationSettings.MainSettings.ConfigStuff[settingsName] = resetValue;

                            string updatedSettingsContent = JsonConvert.SerializeObject(ApplicationSettings.MainSettings, Formatting.Indented);
                            File.WriteAllText(ApplicationSettings.ConfigLocation, updatedSettingsContent);
                            util.println($"Setting '{settingsName}' reset successfully to '{resetValue}'.");
                        }
                        else
                        {
                            util.println($"Error: No default value found for '{settingsName}'.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.info($"{ex.InnerException} Occurred: {ex}");
                        util.println($"Error: Could not reset '{settingsName}'.");
                        util.println($"Details: {ex.Message}");
                    }
                    break;

                case actions.list:
                    if (args.Length > 2)
                    {
                        string groupName = args[2].ToLower();
                        util.println($"Settings for group '{groupName}':");

                        var groupedSettings = ApplicationSettings.MainSettings.ConfigStuff
                            .Where(kvp => kvp.Key.ToLower().StartsWith(groupName + "."))
                            .ToList();

                        if (groupedSettings.Any())
                        {
                            foreach (var setting in groupedSettings)
                            {
                                util.println($"  {setting.Key}: {setting.Value}");
                            }
                        }
                        else
                        {
                            util.println("No settings found for this group.");
                        }
                    }
                    else
                    {
                        util.println("All available settings:");
                        var settingsByGroup = ApplicationSettings.MainSettings.ConfigStuff
                            .GroupBy(kvp => kvp.Key.Split('.')[0])
                            .OrderBy(g => g.Key);

                        foreach (var group in settingsByGroup)
                        {
                            util.println($"\n[{group.Key}]");
                            foreach (var setting in group.OrderBy(s => s.Key))
                            {
                                util.println($"  {setting.Key}: {setting.Value}");
                            }
                        }
                    }
                    break;
            }
        }

        // Enum for different config actions.
        public enum actions
        {
            get,
            set,
            toggle,
            reset,
            list
        }

        // Fetches all releases from GitHub.
        private static async Task<JsonElement> GetReleasesFromGitHub()
        {
            var response = await _httpClient.GetStringAsync(GitHubApiUrl);
            var jsonDoc = JsonDocument.Parse(response);
            return jsonDoc.RootElement;
        }

        // Downloads a file from a URL to a specified path.
        private static async Task DownloadFileAsync(string url, string destinationPath)
        {
            using (var response = await _httpClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }

        // Retrieves the default value for a given setting name.
        private static object GetDefaultValue(string settingsName)
        {
            switch (settingsName.ToLower())
            {
                case "user.firsttime": return true;
                case "dev.debugmode": return false;
                case "core.internalcommandprefix": return "!";
                case "core.enforceprefix": return true;
                case "core.unicodeconversion": return true;
                case "core.showloggingoutput": return false;
                case "cli.maxhistorylength": return 100;
                case "cli.historyupkey": return (int)Settings.HistoryUp.UpArrow;
                case "cli.historydownkey": return (int)Settings.HistoryDown.DownArrow;
                default: return null;
            }
        }

        // Maps a user-friendly key to the actual property name.
        private static string GetPropertyNameFromKey(string settingsName)
        {
            switch (settingsName.ToLower())
            {
                case "user.firsttime": return "firstTime";
                case "dev.debugmode": return "debugMode";
                case "core.internalcommandprefix": return "internalCommandPrefix";
                case "core.enforceprefix": return "enforcePrefix";
                case "core.unicodeconversion": return "unicodeConversion";
                case "core.showloggingoutput": return "ShowLoggingOutput";
                case "cli.maxhistorylength": return "MaxHistoryLength";
                case "cli.historyupkey": return "HistoryUpKey";
                case "cli.historydownkey": return "HistoryDownKey";
                default: return null;
            }
        }
    }
}
