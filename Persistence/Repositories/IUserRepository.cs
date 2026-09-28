using Domain.Authorization;

namespace Persistence.Repositories
{
    public interface IUserRepository
    {
        /// <summary>Zwraca użytkownika po loginie (porównanie bez rozróżniania wielkości liter) albo null.</summary>
        Task<UserAuthInfo?> GetByUserNameAsync(string userName, CancellationToken cancellationToken);

        /// <summary>Zwraca użytkownika po Id albo null. Używane przy odświeżaniu tokenu.</summary>
        Task<UserAuthInfo?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
