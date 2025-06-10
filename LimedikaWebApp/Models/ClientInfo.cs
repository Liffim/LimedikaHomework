using System.ComponentModel.DataAnnotations;

namespace LimedikaWebApp.Models
{
    public class ClientInfo
    {
        [Key]
        public int ClientId { get; set; }
        [Required(ErrorMessage = "Pavadinimas yra privalomas")]
        [StringLength(100, ErrorMessage = "Pavadinimas viršija leistiną kiekį simbolių")]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "Adresas yra privalomas")]
        [StringLength(200, ErrorMessage = "Adresas viršija leistiną kiekį simbolių")]
        public string Address { get; set; }

        [StringLength(20)]
        public string? PostCode { get; set; } // Can be null at the start

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;// also can be approched as null on creation

    }
}
