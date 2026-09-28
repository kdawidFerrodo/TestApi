using System.ComponentModel.DataAnnotations;

namespace Application.Services.Auth
{
    public sealed record LoginRequest(
        [Required, MaxLength(256)] string UserName,
        [Required, MaxLength(256)] string Password);

    public sealed record RefreshTokenRequest(
        [Required, MaxLength(512)] string RefreshToken);

    public sealed record AuthTokensResponse(
        string AccessToken,
        DateTimeOffset AccessTokenExpiresAt,
        string RefreshToken,
        DateTimeOffset RefreshTokenExpiresAt,
        string TokenType = "Bearer");

    public enum AuthError
    {
        InvalidCredentials,
        UserInactive,
        InvalidRefreshToken,
    }

    public sealed class AuthResult
    {
        private AuthResult(AuthTokensResponse? tokens, AuthError? error)
        {
            Tokens = tokens;
            Error = error;
        }

        public AuthTokensResponse? Tokens { get; }

        public AuthError? Error { get; }

        public bool Succeeded => Tokens is not null;

        public static AuthResult Success(AuthTokensResponse tokens) => new(tokens, null);

        public static AuthResult Failure(AuthError error) => new(null, error);
    }
}
