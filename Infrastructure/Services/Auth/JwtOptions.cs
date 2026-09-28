using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.Auth
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Klucz HMAC-SHA256, min. 32 bajty (256 bitów). Nigdy w repozytorium -
        /// ustawiany przez user-secrets (dev) albo zmienną środowiskową Jwt__SigningKey / key vault (prod).
        /// </summary>
        [Required]
        [MinLength(32)]
        public string SigningKey { get; set; } = string.Empty;

        [Range(1, 60)]
        public int AccessTokenLifetimeMinutes { get; set; } = 15;

        [Range(1, 90)]
        public int RefreshTokenLifetimeDays { get; set; } = 7;
    }
}
