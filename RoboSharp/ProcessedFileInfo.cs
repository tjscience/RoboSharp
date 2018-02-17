namespace RoboSharp
{
    public enum FileClassType
    {
        NewDir,
        File,
        SystemMessage
    }

    public class ProcessedFileInfo
    {
        public string FileClass { get; set; }
        public FileClassType FileClassType { get; set; }
        public long Size { get; set; }
        public string Name { get; set; }
    }
}
