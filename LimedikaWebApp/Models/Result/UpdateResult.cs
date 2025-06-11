namespace LimedikaWebApp.Models.Result
{
    public class UpdateResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int UpdatedCount { get; set; }
        public int FailedCount { get; set; }
    }
}
