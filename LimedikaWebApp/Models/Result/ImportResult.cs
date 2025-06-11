namespace LimedikaWebApp.Models.Result
{
    public class ImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }

    }
}
