using System.ComponentModel.DataAnnotations;

namespace LimedikaWebApp.Models
{
    public enum  ActionType
    {
        ClientCreated,
        Error,
        PostCodeUpdated,
        PostCodeUpdatedCompleted,
        ImportCompleted,
        ImportStarted
    }
    public class LogEntry
    {
        [Key]
        public int LogId { get; set; }
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        public ActionType Action { get; set; }

        [Required]
        public string Details { get; set; }// Pvz., "Klientas 'UAB X' importuotas." arba "Klientui ID 5 atnaujintas pašto kodas į LT-12345."

        public int? ClientId { get; set; } // Nullable, because some actions may not be related to a specific client
        public string? ClientName { get; set; } // Nullable, because some actions may not be related to a specific client
    }
}
