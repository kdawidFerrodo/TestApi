using Domain.Entities;

namespace Persistence.Repositories
{
    /// <summary>
    /// Tymczasowa implementacja w pamięci - tokeny znikają po restarcie i nie działa przy kilku instancjach API.
    /// Docelowo tabela RefreshTokens (Id, UserId, TokenHash [unique index], FamilyId [index], CreatedAt,
    /// ExpiresAt, RevokedAt, ReplacedByTokenHash) + okresowe czyszczenie wygasłych wierszy.
    /// </summary>
    public sealed class InMemoryRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly object _sync = new();
        private readonly Dictionary<string, RefreshToken> _tokensByHash = new(StringComparer.Ordinal);

        public Task AddAsync(RefreshToken token, CancellationToken cancellationToken)
        {
            lock (_sync)
            {
                _tokensByHash.Add(token.TokenHash, token);
            }

            return Task.CompletedTask;
        }

        public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            lock (_sync)
            {
                return Task.FromResult(_tokensByHash.GetValueOrDefault(tokenHash));
            }
        }

        public Task<bool> TryRevokeAsync(Guid tokenId, DateTimeOffset now, string? replacedByTokenHash, CancellationToken cancellationToken)
        {
            lock (_sync)
            {
                var token = _tokensByHash.Values.FirstOrDefault(t => t.Id == tokenId);
                if (token is null || token.RevokedAt is not null)
                {
                    return Task.FromResult(false);
                }

                token.RevokedAt = now;
                token.ReplacedByTokenHash = replacedByTokenHash;
                return Task.FromResult(true);
            }
        }

        public Task RevokeFamilyAsync(Guid familyId, DateTimeOffset now, CancellationToken cancellationToken)
        {
            return RevokeWhere(t => t.FamilyId == familyId, now);
        }

        public Task RevokeAllForUserAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
        {
            return RevokeWhere(t => t.UserId == userId, now);
        }

        private Task RevokeWhere(Func<RefreshToken, bool> predicate, DateTimeOffset now)
        {
            lock (_sync)
            {
                foreach (var token in _tokensByHash.Values.Where(t => t.RevokedAt is null && predicate(t)))
                {
                    token.RevokedAt = now;
                }
            }

            return Task.CompletedTask;
        }
    }
}
