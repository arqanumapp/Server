using System.ComponentModel.DataAnnotations;

namespace ArqanumServer.Models.Entities
{
    public class Account
    {
        public string Id { get; set; }

        [MaxLength(32)] public string Username { get; set; }

        [MaxLength(32)] public string? FirstName { get; set; }

        [MaxLength(32)] public string? LastName { get; set; }

        [MaxLength(150)] string? Bio { get; set; }

        public byte[] SignaturePublicKey { get; set; }
    }
}
