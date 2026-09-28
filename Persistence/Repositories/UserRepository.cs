using Domain.Authorization;

namespace Persistence.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        // TODO: pobranie użytkownika z bazy. Zwróć UserAuthInfo z:
        //  - PasswordHash w formacie PasswordHasher z ASP.NET Core Identity (patrz Infrastructure.Services.Auth.PasswordHasher.Hash),
        //  - rolami użytkownika,
        //  - spłaszczoną listą uprawnień ze wszystkich jego ról (bez duplikatów).
        public Task<UserAuthInfo?> GetByUserNameAsync(string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Dodaj zapytanie o użytkownika po loginie.");
        }

        public Task<UserAuthInfo?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Dodaj zapytanie o użytkownika po Id.");
        }
    }
}
