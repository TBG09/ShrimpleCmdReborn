using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;

// Assumed namespaces for update functionality
using ShrimpleCmd.dev;
using ShrimpleCmd.log;

namespace ShrimpleCmd.utils
{
    public class util
    {
        // HttpClient instance for making web requests. Moved from shrimple.cs for reusability.
        private static readonly HttpClient _httpClient = new HttpClient();
        // The GitHub API URL for fetching release information. Moved from shrimple.cs.
        private const string GitHubApiUrl = "https://api.github.com/repos/TBG09/ShrimpleCmdReborn/releases";

        public static void print(string msg)
        {
            Console.Write(msg);
        }
        public static void println(string msg)
        {
            Console.WriteLine(msg);
        }
        public static string read()
        {
            return Console.ReadLine();
        }

        // what am i doing here?
        public static string getExceptionTy(Exception ex, Settings settings)
        {
            if (settings.debugMode)
            {
                return ex.ToString();
            }
            else
            {
                return ex.GetType().Name + "\n" + ex.Message;
            }
        }

        public static string ConvertToSymbols(string text)
        {

            string unicodePattern = @"\\u[0-9a-fA-F]{4,6}";
            return Regex.Replace(text, unicodePattern, match =>
            {
                try
                {
                    string hexValue = match.Value.Substring(2);

                    int charCode = int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);

                    return char.ConvertFromUtf32(charCode);
                }
                catch (Exception)
                {

                    return match.Value;
                }
            });
        }


        public static string ConvertToUnicodeString(string text)
        {
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c > 127)
                {

                    sb.Append("\\u");
                    sb.AppendFormat("{0:x4}", (int)c);
                }
                else
                {

                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Checks for the latest available version on GitHub.
        /// </summary>
        /// <returns>The latest version string if an update is available; otherwise, null.</returns>
        public static async Task<string> CheckForUpdatesAsync()
        {
            try
            {
                // Set a user agent for the GitHub API requests. This is required.
                _httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ShrimpleCmd", "1.0"));
                
                var releases = await GetReleasesFromGitHub();

                // Check if the JsonElement is an array and not empty
                if (releases.ValueKind != JsonValueKind.Array || !releases.EnumerateArray().Any())
                {
                    return null; // No releases found
                }

                // Get the latest non-prerelease.
                var latestRelease = releases.EnumerateArray().FirstOrDefault(r => !r.GetProperty("prerelease").GetBoolean());
                
                if (latestRelease.ValueKind == JsonValueKind.Undefined)
                {
                    return null; // No proper release found
                }
                
                string latestVersion = latestRelease.GetProperty("tag_name").GetString();
                string currentVersion = ApplicationSettings.Version;

                // Compare versions. A simple string comparison can work for tag names like "v1.0.0".
                // For more complex versioning, a Version class comparison would be better.
                if (string.Compare(latestVersion, currentVersion, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    return latestVersion; // Update available
                }
                else
                {
                    return null; // No update available
                }
            }
            catch (Exception ex)
            {
                // Log the exception but return null so the application doesn't crash.
                println($"An error occurred while checking for updates: {ex.Message}");
                return null;
            }
        }
        // look what i found out, summaries!

        /// <summary>
        /// Downloads and installs a specific version or the latest update.
        /// </summary>
        /// <param name="targetVersion">The specific version to install. If null, installs the latest.</param>
        /// <param name="includePrerelease">Whether to consider pre-release versions.</param>
        public static async Task InstallUpdateAsync(string targetVersion, bool includePrerelease)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("ShrimpleCmd", "1.0"));

                var releases = await GetReleasesFromGitHub();

                if (releases.ValueKind != JsonValueKind.Array || !releases.EnumerateArray().Any())
                {
                    util.println("No proper releases found on GitHub.");
                    return;
                }

                JsonElement selectedRelease;
                if (!string.IsNullOrEmpty(targetVersion))
                {
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

                // best way i could think of
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
        
        private static async Task<JsonElement> GetReleasesFromGitHub()
        {
            var response = await _httpClient.GetStringAsync(GitHubApiUrl);
            var jsonDoc = JsonDocument.Parse(response);
            return jsonDoc.RootElement;
        }

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
    }
}
