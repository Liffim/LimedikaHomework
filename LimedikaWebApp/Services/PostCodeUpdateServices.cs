using LimedikaWebApp.Models;
using LimedikaWebApp.Data;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Text.Json;
using LimedikaWebApp.Infrastructure.JsonConverters;
using Microsoft.EntityFrameworkCore;
using System.Web;
using System.Net;
using System.Text.Json.Serialization;

namespace LimedikaWebApp.Services
{
    public class PostCodeUpdateServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LogService _logService;
        private readonly PostItApiSettings _apiSettings;

        public PostCodeUpdateServices(ApplicationDbContext context, LogService logService, IOptions<PostItApiSettings> apiSettings, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logService = logService;
            _apiSettings = apiSettings.Value;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<UpdateResult> UpdateAllClientsPostCOdeAsync()
        {
            await _logService.LogActionAsync(ActionType.PostCodeUpdated, "Pradėtas klientų pašto kodų atnaujinimas iš PostIt API.");
            var clientsToUpdate = await _context.Clients.Where(client => string.IsNullOrEmpty(client.PostCode)).ToListAsync();

            if (!clientsToUpdate.Any())
            {
                await _logService.LogActionAsync(ActionType.PostCodeUpdatedCompleted, "Nėra klientų, kuriems reikia atnaujinti pašto kodus.");
                return new UpdateResult
                {
                    Success = false,
                    Message = "Nėra klientų, kuriems reikia atnaujinti pašto kodus.",
                };
            }

            int updatedCount = 0;
            int failedCount = 0;
            // Best practice: Add a User-Agent header in Program.cs when configuring AddHttpClient
            var httpClient = _httpClientFactory.CreateClient();

            foreach (var client in clientsToUpdate)
            {
                // Ensure the address is not null or empty before making a request
                if (string.IsNullOrWhiteSpace(client.Address))
                {
                    await _logService.LogActionAsync(ActionType.PostCodeUpdated, $"Klientas {client.ClientName} neturi adreso, pašto kodas neatnaujintas.");
                    failedCount++;
                    continue;
                }

                // FIX #1: Use WebUtility for encoding instead of the old HttpUtility
                string encodedSearchTerm = WebUtility.UrlEncode(client.Address);

                // FIX #2: Use the correct API parameter 'term' instead of 'search'
                string requestUrl = $"{_apiSettings.BaseUrl}?term={encodedSearchTerm}&key={_apiSettings.ApiKey}";

                try
                {
                    var apiResponse = await httpClient.GetFromJsonAsync<PostItApiResponse>(requestUrl);

                    if (apiResponse != null && apiResponse.Success && apiResponse.Data != null && apiResponse.Data.Any())
                    {
                        // The API might return multiple results, taking the first one is a reasonable approach
                        var postCodeData = apiResponse.Data.First().Post_Code;
                        if (!string.IsNullOrEmpty(postCodeData))
                        {
                            client.PostCode = postCodeData;
                            client.UpdatedAt = DateTime.Now;
                            // Note: Marking as modified is more explicit than Update
                            _context.Entry(client).State = EntityState.Modified;
                            await _logService.LogActionAsync(ActionType.PostCodeUpdated, $"Atnaujintas pašto kodas klientui '{client.ClientName}' į {postCodeData}.");
                            updatedCount++;
                        }
                        else
                        {
                            await _logService.LogActionAsync(ActionType.PostCodeUpdated, $"PostIt API grąžino sėkmingą atsakymą, bet be pašto kodo duomenų klientui '{client.ClientName}'.");
                            failedCount++;
                        }
                    }
                    else
                    {
                        string errorMessage = apiResponse?.Message ?? "API negrąžino sėkmingo atsakymo arba grąžino tuščius duomenis.";
                        await _logService.LogActionAsync(ActionType.PostCodeUpdated, $"Nepavyko gauti pašto kodo klientui '{client.ClientName}' pagal adresą '{client.Address}'. API atsakymas: {errorMessage}");
                        failedCount++;
                    }
                }
                catch (HttpRequestException ex)
                {
                    await _logService.LogActionAsync(ActionType.PostCodeUpdated, $"Tinklo klaida siunčiant užklausą klientui {client.ClientName}: {ex.Message}");
                    failedCount++;
                }
                catch (JsonException ex)
                {
                    await _logService.LogActionAsync(ActionType.PostCodeUpdated, $"Klaida apdorojant (deserializuojant) PostIt API atsakymą klientui {client.ClientName}: {ex.Message}");
                    failedCount++;
                }
                catch (Exception ex)
                {
                    await _logService.LogActionAsync(ActionType.PostCodeUpdated, $"Bendra klaida atnaujinant pašto kodą klientui {client.ClientName}: {ex.Message}");
                    failedCount++;
                }

                // Delay to avoid hitting API rate limits
                await Task.Delay(100);
            }

            await _context.SaveChangesAsync();
            string completionMessage = $"Atnaujinta {updatedCount} klientų pašto kodų, nepavyko atnaujinti {failedCount} klientų.";
            await _logService.LogActionAsync(ActionType.PostCodeUpdatedCompleted, completionMessage);

            return new UpdateResult
            {
                Success = true,
                Message = completionMessage,
                UpdatedCount = updatedCount,
                FailedCount = failedCount
            };
        }
    }
    public class PostItApiResponse
    {
        public bool Success { get; set; }

        // FIX #3: Apply the custom converter to handle the ambiguous 'data' field
        [JsonConverter(typeof(SingleOrArrayConverter<PostItApiData>))]
        public List<PostItApiData> Data { get; set; }

        public string Message { get; set; }
    }

    public class PostItApiData
    {
        [JsonPropertyName("post_code")] // Explicitly map the JSON property name
        public string Post_Code { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
    }

    public class UpdateResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int UpdatedCount { get; set; }
        public int FailedCount { get; set; }
    }
}
