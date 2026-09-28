using Domain.Authorization;
using Domain.Entities;

namespace Infrastructure.Services.Auth
{
    public interface ITokenService
    {
        /// <summary>Tworzy krótko żyjący, podpisany JWT z tożsamością, rolami i uprawnieniami użytkownika.</summary>
        AccessToken CreateAccessToken(UserAuthInfo user);

        /// <summary>
        /// Tworzy nowy refresh token w ramach sesji <paramref name="familyId"/>.
        /// RawToken trafia do klienta, Entity (z samym hashem) do bazy.
        /// </summary>
        (string RawToken, RefreshToken Entity) CreateRefreshToken(Guid userId, Guid familyId);

        string HashRefreshToken(string rawToken);
    }

    public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
}
