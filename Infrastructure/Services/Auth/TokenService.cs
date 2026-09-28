using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Authorization;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Auth
{
    public sealed class TokenService : ITokenService
    {
        private const int RefreshTokenSizeInBytes = 64;

        private readonly JwtOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly SigningCredentials _signingCredentials;
        private readonly JsonWebTokenHandler _tokenHandler = new();

        public TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
        {
            _options = options.Value;
            _timeProvider = timeProvider;
            _signingCredentials = new SigningCredentials(CreateSigningKey(_options.SigningKey), SecurityAlgorithms.HmacSha256);
        }

        public static SymmetricSecurityKey CreateSigningKey(string signingKey) => new(Encoding.UTF8.GetBytes(signingKey));

        public AccessToken CreateAccessToken(UserAuthInfo user)
        {
            var now = _timeProvider.GetUtcNow();
            var expiresAt = now.AddMinutes(_options.AccessTokenLifetimeMinutes);

            var claims = new List<Claim>
            {
                new(AppClaimTypes.Subject, user.Id.ToString()),
                new(AppClaimTypes.Name, user.UserName),
                new(AppClaimTypes.TokenId, Guid.NewGuid().ToString()),
            };
            claims.AddRange(user.Roles.Select(role => new Claim(AppClaimTypes.Role, role)));
            claims.AddRange(user.Permissions.Select(permission => new Claim(AppClaimTypes.Permission, permission)));

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Subject = new ClaimsIdentity(claims),
                IssuedAt = now.UtcDateTime,
                NotBefore = now.UtcDateTime,
                Expires = expiresAt.UtcDateTime,
                SigningCredentials = _signingCredentials,
            };

            return new AccessToken(_tokenHandler.CreateToken(descriptor), expiresAt);
        }

        public (string RawToken, RefreshToken Entity) CreateRefreshToken(Guid userId, Guid familyId)
        {
            var now = _timeProvider.GetUtcNow();
            var rawToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(RefreshTokenSizeInBytes));

            var entity = new RefreshToken
            {
                UserId = userId,
                FamilyId = familyId,
                TokenHash = HashRefreshToken(rawToken),
                CreatedAt = now,
                ExpiresAt = now.AddDays(_options.RefreshTokenLifetimeDays),
            };

            return (rawToken, entity);
        }

        // Token ma 512 bitów losowości, więc wystarczy szybki SHA-256 bez soli (w przeciwieństwie do haseł).
        public string HashRefreshToken(string rawToken) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
    }
}
