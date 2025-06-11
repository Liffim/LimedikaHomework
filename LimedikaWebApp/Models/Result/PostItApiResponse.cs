using LimedikaWebApp.Infrastructure.JsonConverters;
using LimedikaWebApp.Services;
using System.Text.Json.Serialization;

namespace LimedikaWebApp.Models.Result
{
    public class PostItApiResponse
    {
        public bool Success { get; set; }

        [JsonConverter(typeof(SingleOrArrayConverter<PostItApiData>))]
        public List<PostItApiData> Data { get; set; }

        public string Message { get; set; }
    }
}
