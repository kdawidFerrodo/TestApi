using Application.Services.Auth;
using Domain.Authorization;
using FerrodoERPApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FerrodoERPApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        [HttpPost("login")]
        [ProducesResponseType<AuthTokensResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
        {
            return ToActionResult(await _authService.LoginAsync(request, cancellationToken));
        }

        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        [HttpPost("refresh")]
        [ProducesResponseType<AuthTokensResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            return ToActionResult(await _authService.RefreshAsync(request.RefreshToken, cancellationToken));
        }

        // Anonimowy, bo access token mógł już wygasnąć - dowodem jest posiadanie refresh tokenu.
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Auth)]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
            return NoContent();
        }

        [HttpPost("logout-all")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
        {
            await _authService.LogoutAllAsync(GetCurrentUserId(), cancellationToken);
            return NoContent();
        }

        [HttpGet("me")]
        [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
        public ActionResult<CurrentUserResponse> Me()
        {
            return new CurrentUserResponse(
                GetCurrentUserId(),
                User.Identity?.Name ?? string.Empty,
                User.FindAll(AppClaimTypes.Role).Select(c => c.Value).ToArray(),
                User.FindAll(AppClaimTypes.Permission).Select(c => c.Value).ToArray());
        }

        private Guid GetCurrentUserId() => Guid.Parse(User.FindFirst(AppClaimTypes.Subject)!.Value);

        private IActionResult ToActionResult(AuthResult result)
        {
            if (result.Succeeded)
            {
                return Ok(result.Tokens);
            }

            // Jeden komunikat dla złego loginu i złego hasła - nie zdradzamy, czy login istnieje.
            var detail = result.Error switch
            {
                AuthError.InvalidCredentials => "Nieprawidłowy login lub hasło.",
                AuthError.UserInactive => "Konto jest nieaktywne.",
                _ => "Sesja wygasła. Zaloguj się ponownie.",
            };

            return Problem(detail: detail, statusCode: StatusCodes.Status401Unauthorized);
        }
    }

    public sealed record CurrentUserResponse(Guid Id, string UserName, string[] Roles, string[] Permissions);
}
