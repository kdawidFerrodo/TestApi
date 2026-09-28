using Domain.Entities;

namespace Persistence.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token, CancellationToken cancellationToken);

        Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);

        /// <summary>
        /// Atomowo unieważnia token, o ile jest jeszcze aktywny.
        /// Zwraca false, jeśli ktoś zdążył go już użyć (np. równoległe żądanie).
        /// W SQL: UPDATE ... SET RevokedAt = @now, ReplacedByTokenHash = @hash WHERE Id = @id AND RevokedAt IS NULL
        /// i sprawdzenie liczby zmienionych wierszy.
        /// </summary>
        Task<bool> TryRevokeAsync(Guid tokenId, DateTimeOffset now, string? replacedByTokenHash, CancellationToken cancellationToken);

        /// <summary>Unieważnia wszystkie aktywne tokeny sesji (wylogowanie / wykrycie kradzieży tokenu).</summary>
        Task RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken cancellationToken);

        /// <summary>Unieważnia wszystkie aktywne tokeny użytkownika (wyloguj ze wszystkich urządzeń, zmiana hasła, blokada konta).</summary>
        Task RevokeAllForUserAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken);
    }
}
