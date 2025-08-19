namespace ShrimpleCmd.utils
{
    public class vars
    {
        public static string versionChange = @"Changes/Additions:
        • Added bullet points to the change log :D
        • Added unicode conversion support via the \u sequence. eg: \u2717 = ✗. This can be disabled via the config via unicodeConversion.
        • Added History, can be configured via MaxHistoryLength. To configure the history up and down keys, you can modify HistoryDownKey and HistoryUpKey, for all the possible types, visit https://learn.microsoft.com/en-us/dotnet/api/system.consolekey?view=net-9.0
        • Added a config command, basic config managment.
        • Logging has been added, to show it on the screen modify ShowLoggingOutput.
        • Can now run executables, and also allowing choices for executables with the same name, supports exe,bat and cmd files.
        • New shrimple command, main manager for most main application operations.
        Technical Stuff:
        • Now using Path.Combine for configLocation.
        • Added IssuesSuccess CheckStatus.
        • Added logging E V E R Y W H E R E :D
        • Fixed the internal assembly version from 0.0.1 to 0.1.1
        • Added comments alot in some places i think(he thinks)
        • Added two function in utils for versions, will come in properly to an update in the next version.
        ";
    }
}