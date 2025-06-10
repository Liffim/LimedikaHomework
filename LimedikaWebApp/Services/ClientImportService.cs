using LimedikaWebApp.Models;
using LimedikaWebApp.Data;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

namespace LimedikaWebApp.Services
{
    public class ClientImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly LogService _logService;
        
        public ClientImportService(ApplicationDbContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<ImportResult> ImportClientsAsync(Stream filestream)
        {
            await _logService.LogActionAsync(Models.ActionType.ClientCreated, "Pradėtas klientų importas iš įkelto failo.");// CHANGE TO IMPORT START

            var clientsToImport = await System.Text.Json.JsonSerializer.DeserializeAsync<List<ClientDto>>(filestream, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (clientsToImport == null || !clientsToImport.Any())
            {
                await _logService.LogActionAsync(Models.ActionType.Error, "Nėra klientų duomenų importavimui."); // CHANGE TO FAILED
                return new ImportResult
                {
                    Success = true,
                    Message = "Nėra klientų duomenų importavimui.",
                    ImportedCount = 0,
                    SkippedCount = 0
                };
            }

            int importedCount = 0;
            int skippedCount = 0;

            

            foreach (var clientDto in clientsToImport)
            {
                bool exists = _context.Clients.Any(c => c.ClientName == clientDto.Name && c.Address == clientDto.Address);
                if (!exists)
                {
                    var client = new ClientInfo
                    {
                        ClientName = clientDto.Name,
                        Address = clientDto.Address,
                        PostCode = string.IsNullOrEmpty(clientDto.PostCode) ? null : clientDto.PostCode,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Clients.Add(client);
                    await _context.SaveChangesAsync();
                    await _logService.LogActionAsync(Models.ActionType.ClientCreated, $"Klientas '{client.ClientName}' importuotas.", client.ClientId);
                    importedCount++;
                }
                else
                {
                    skippedCount++;
                    await _logService.LogActionAsync(Models.ActionType.Error, $"Klientas '{clientDto.Name}' su adresu '{clientDto.Address}' jau egzistuoja. Praleistas importas.");
                }
            }
            string successMessage = $"Importas baigtas. Iš viso importuota {importedCount} klientų, praleista {skippedCount} klientų.";
            await _logService.LogActionAsync(Models.ActionType.ClientCreated, successMessage);
            return new ImportResult
            {
                Success = true,
                Message = successMessage,
                ImportedCount = importedCount,
                SkippedCount = skippedCount
            };
        }

        public class ClientDto
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public string PostCode { get; set; }
        }
        public class ImportResult { 
            public bool Success { get; set; }
            public string Message { get; set; }
            public int ImportedCount { get; set; }
            public int SkippedCount { get; set; }

        }
    }
}
