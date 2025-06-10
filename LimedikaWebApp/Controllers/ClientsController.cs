using LimedikaWebApp.Data;
using LimedikaWebApp.Models;
using LimedikaWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LimedikaWebApp.Controllers
{
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LogService _logService;
        private readonly PostCodeUpdateServices _postCodeUpdateServices;
        private readonly ClientImportService _clientImportService;

        public ClientsController(ApplicationDbContext context, LogService logService, PostCodeUpdateServices postCodeUpdateServices, ClientImportService clientImportService)
        {
            _context = context;
            _logService = logService;
            _postCodeUpdateServices = postCodeUpdateServices;
            _clientImportService = clientImportService;
        }

        //GET: clients
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients.ToListAsync();
            ViewBag.Message = TempData["Message"];
            return View(clients);
        }

        // POST: ImportClients
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportClients(IFormFile jsonFile)
        {
            // 1. Patikriname, ar failas buvo įkeltas
            if (jsonFile == null || jsonFile.Length == 0)
            {
                TempData["Message"] = "Klaida: failas nepasirinktas arba tuščias.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Patikriname, ar failo plėtinys yra .json
            if (Path.GetExtension(jsonFile.FileName).ToLower() != ".json")
            {
                TempData["Message"] = "Klaida: galima įkelti tik .json formato failus.";
                return RedirectToAction(nameof(Index));
            }

            // 3. Atidarome failo srautą (stream) ir perduodame jį servisui
            try
            {
                using var stream = jsonFile.OpenReadStream();
                var result = await _clientImportService.ImportClientsAsync(stream);
                TempData["Message"] = result.Message;
            }
            catch (Exception ex)
            {
                // Pagrindinė klaidų gaudyklė, jei servise įvyktų nenumatyta klaida
                await _logService.LogActionAsync(Models.ActionType.Error, $"Kritinė klaida importuojant failą: {ex.Message}");
                TempData["Message"] = "Įvyko nenumatyta klaida apdorojant failą.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: UpdatePostCodes
        [HttpPost]
        public async Task<IActionResult> UpdatePostCodes()
        {
            var result = await _postCodeUpdateServices.UpdateAllClientsPostCOdeAsync();
            TempData["Message"] = result.Success ? $"{result.UpdatedCount} klientų pašto kodai atnaujinti." : result.Message;
            return RedirectToAction("Index");
        }
    }
}
