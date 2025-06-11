using System.Text.Json.Serialization;

namespace LimedikaWebApp.Models.Result
{
    public class PostItApiData
    {
        [JsonPropertyName("post_code")] // Explicitly map the JSON property name
        public string Post_Code { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
    }
}
