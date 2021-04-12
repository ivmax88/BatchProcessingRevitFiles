namespace BatchProcessingRevitFilesCore
{
    public enum Status
    {
        None = 0,
        RevitStarting,
        RevitStarted,
        RevitFileOpening,
        RevitFileOpened,
        ScriptStarted,
        ScriptFinished,
        RevitFileClosed
    }
}
