using LimedikaWebApp.Models;
using LimedikaWebApp.Data;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;
using LimedikaWebApp.Models.Result;
using LimedikaWebApp.Models.DTO;
using Microsoft.EntityFrameworkCore;

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
            await _logService.LogActionAsync(Models.ActionType.ImportStarted, "Pradėtas klientų importas iš įkelto failo.");// CHANGE TO IMPORT START

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
            var newClientsToLog = new List<ClientInfo>();
            var existingClients = await _context.Clients
                .Select(c => c.ClientName + "|" + c.Address)
                .ToHashSetAsync();


            foreach (var clientDto in clientsToImport)
            {
                string clientKey = clientDto.Name + "|" + clientDto.Address;
                if (!existingClients.Contains(clientKey))
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
                    newClientsToLog.Add(client);
                    importedCount++;
                }
                else
                {
                    skippedCount++;
                    await _logService.LogActionAsync(Models.ActionType.Error, $"Klientas '{clientDto.Name}' su adresu '{clientDto.Address}' jau egzistuoja. Praleistas importas.");
                }
            }
            await _context.SaveChangesAsync();

            //logging all added clients
            foreach (var savedClient in newClientsToLog)
            {
                await _logService.LogActionAsync(
                    ActionType.ClientCreated,
                    $"Klientas '{savedClient.ClientName}' importuotas.",
                    savedClient.ClientId,
                    savedClient.ClientName
                );
            }

            string successMessage = $"Importas baigtas. Iš viso importuota {importedCount} klientų, praleista {skippedCount} klientų duplikatų.";
            await _logService.LogActionAsync(Models.ActionType.ImportCompleted, successMessage);
            return new ImportResult
            {
                Success = true,
                Message = successMessage,
                ImportedCount = importedCount,
                SkippedCount = skippedCount
            };
        }


        
    }
}
