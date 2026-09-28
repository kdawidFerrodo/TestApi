namespace Application.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

        Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

        /// <summary>Wylogowanie bieżącej sesji (urządzenia), do której należy refresh token.</summary>
        Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);

        /// <summary>Wylogowanie ze wszystkich urządzeń.</summary>
        Task LogoutAllAsync(Guid userId, CancellationToken cancellationToken);
    }
}
