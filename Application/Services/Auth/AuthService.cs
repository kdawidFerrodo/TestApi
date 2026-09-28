using Domain.Authorization;
using Domain.Entities;
using Infrastructure.Services.Auth;
using Microsoft.Extensions.Logging;
using Persistence.Repositories;

namespace Application.Services.Auth
{
    public sealed class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository users,
            IRefreshTokenRepository refreshTokens,
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            TimeProvider timeProvider,
            ILogger<AuthService> logger)
        {
            _users = users;
            _refreshTokens = refreshTokens;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _users.GetByUserNameAsync(request.UserName, cancellationToken);
            if (user is null)
            {
                _passwordHasher.VerifyDummy(request.Password);
                _logger.LogWarning("Nieudane logowanie: nieznany login {UserName}", request.UserName);
                return AuthResult.Failure(AuthError.InvalidCredentials);
            }

            if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
            {
                // TODO (baza): licznik nieudanych prób i czasowa blokada konta.
                _logger.LogWarning("Nieudane logowanie: błędne hasło dla użytkownika {UserId}", user.Id);
                return AuthResult.Failure(AuthError.InvalidCredentials);
            }

            // Sprawdzane dopiero po poprawnym haśle, żeby nie zdradzać stanu konta osobom bez hasła.
            if (!user.IsActive)
            {
                return AuthResult.Failure(AuthError.UserInactive);
            }

            _logger.LogInformation("Użytkownik {UserId} zalogowany", user.Id);
            return AuthResult.Success(await IssueTokensAsync(user, familyId: Guid.NewGuid(), cancellationToken));
        }

        public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var now = _timeProvider.GetUtcNow();
            var stored = await _refreshTokens.GetByHashAsync(_tokenService.HashRefreshToken(refreshToken), cancellationToken);

            if (stored is null)
            {
                return AuthResult.Failure(AuthError.InvalidRefreshToken);
            }

            if (stored.RevokedAt is not null)
            {
                // Token już raz wymieniony, a ktoś używa go ponownie -> prawdopodobnie wyciekł.
                // Nie wiemy, kto jest prawowitym właścicielem, więc zabijamy całą sesję.
                if (stored.ReplacedByTokenHash is not null)
                {
                    await RevokeFamilyAfterReuseAsync(stored, now, cancellationToken);
                }

                return AuthResult.Failure(AuthError.InvalidRefreshToken);
            }

            if (stored.ExpiresAt <= now)
            {
                return AuthResult.Failure(AuthError.InvalidRefreshToken);
            }

            // Użytkownik pobierany ponownie z bazy: nowy access token ma aktualne role/uprawnienia,
            // a zablokowane konto przestaje działać najpóźniej po wygaśnięciu access tokenu.
            var user = await _users.GetByIdAsync(stored.UserId, cancellationToken);
            if (user is null || !user.IsActive)
            {
                await _refreshTokens.RevokeFamilyAsync(stored.FamilyId, now, cancellationToken);
                return AuthResult.Failure(AuthError.InvalidRefreshToken);
            }

            // Rotacja: stary token unieważniony, nowy w tej samej rodzinie.
            var (rawToken, newEntity) = _tokenService.CreateRefreshToken(user.Id, stored.FamilyId);
            if (!await _refreshTokens.TryRevokeAsync(stored.Id, now, newEntity.TokenHash, cancellationToken))
            {
                // Ten sam token został właśnie użyty przez inne żądanie - traktujemy jak ponowne użycie.
                await RevokeFamilyAfterReuseAsync(stored, now, cancellationToken);
                return AuthResult.Failure(AuthError.InvalidRefreshToken);
            }

            await _refreshTokens.AddAsync(newEntity, cancellationToken);
            return AuthResult.Success(CreateResponse(user, rawToken, newEntity));
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var stored = await _refreshTokens.GetByHashAsync(_tokenService.HashRefreshToken(refreshToken), cancellationToken);
            if (stored is not null)
            {
                await _refreshTokens.RevokeFamilyAsync(stored.FamilyId, _timeProvider.GetUtcNow(), cancellationToken);
            }
        }

        public Task LogoutAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            return _refreshTokens.RevokeAllForUserAsync(userId, _timeProvider.GetUtcNow(), cancellationToken);
        }

        private async Task<AuthTokensResponse> IssueTokensAsync(UserAuthInfo user, Guid familyId, CancellationToken cancellationToken)
        {
            var (rawToken, entity) = _tokenService.CreateRefreshToken(user.Id, familyId);
            await _refreshTokens.AddAsync(entity, cancellationToken);
            return CreateResponse(user, rawToken, entity);
        }

        private AuthTokensResponse CreateResponse(UserAuthInfo user, string rawRefreshToken, RefreshToken refreshToken)
        {
            var accessToken = _tokenService.CreateAccessToken(user);
            return new AuthTokensResponse(accessToken.Token, accessToken.ExpiresAt, rawRefreshToken, refreshToken.ExpiresAt);
        }

        private async Task RevokeFamilyAfterReuseAsync(RefreshToken token, DateTimeOffset now, CancellationToken cancellationToken)
        {
            _logger.LogWarning(
                "Ponowne użycie refresh tokenu użytkownika {UserId}, unieważniam sesję {FamilyId}",
                token.UserId,
                token.FamilyId);
            await _refreshTokens.RevokeFamilyAsync(token.FamilyId, now, cancellationToken);
        }
    }
}
