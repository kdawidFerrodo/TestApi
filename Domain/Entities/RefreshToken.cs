namespace Domain.Entities
{
    /// <summary>
    /// Refresh token przechowywany po stronie serwera.
    /// W bazie trzymamy wyłącznie hash tokenu - wyciek tabeli nie pozwala przejąć sesji.
    /// </summary>
    public sealed class RefreshToken
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required Guid UserId { get; init; }

        /// <summary>SHA-256 z wartości tokenu wysłanej klientowi.</summary>
        public required string TokenHash { get; init; }

        /// <summary>
        /// Identyfikator "rodziny" - wszystkie tokeny powstałe przez rotację z jednego logowania
        /// (jedna sesja / jedno urządzenie). Pozwala unieważnić całą sesję naraz.
        /// </summary>
        public required Guid FamilyId { get; init; }

        public required DateTimeOffset CreatedAt { get; init; }

        public required DateTimeOffset ExpiresAt { get; init; }

        public DateTimeOffset? RevokedAt { get; set; }

        /// <summary>Hash tokenu, który zastąpił ten przy rotacji (null = unieważniony bez następcy).</summary>
        public string? ReplacedByTokenHash { get; set; }

        public bool IsActive(DateTimeOffset now) => RevokedAt is null && now < ExpiresAt;
    }
}
