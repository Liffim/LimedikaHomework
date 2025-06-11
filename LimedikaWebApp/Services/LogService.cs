using LimedikaWebApp.Models;
using LimedikaWebApp.Data;
using System.Threading.Tasks;

namespace LimedikaWebApp.Services
{
    public class LogService
    {
        private readonly ApplicationDbContext _context;
        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task LogActionAsync(ActionType action, string details, int? clientId = null, string? clientName = null)
        {
            var logEntry = new LogEntry
            {
                Action = action,
                Details = details,
                ClientId = clientId,
                ClientName = clientName
            };
            _context.LogEntries.Add(logEntry);
            await _context.SaveChangesAsync();
        }
    }
}
