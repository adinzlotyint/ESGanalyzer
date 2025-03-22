using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace ESGanalyzer.Backend.Models {
    public class User {
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
