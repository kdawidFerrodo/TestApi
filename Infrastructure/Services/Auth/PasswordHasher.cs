using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Auth
{
    /// <summary>
    /// Hashowanie haseł algorytmem PBKDF2 (HMAC-SHA512, sól, 100 000 iteracji) z ASP.NET Core Identity.
    /// </summary>
    public sealed class PasswordHasher : IPasswordHasher
    {
        private static readonly PasswordHasher<object> Hasher = new();
        private static readonly Lazy<string> DummyHash = new(() => Hasher.HashPassword(new object(), Guid.NewGuid().ToString()));

        public string Hash(string password) => Hasher.HashPassword(new object(), password);

        public bool Verify(string passwordHash, string password) =>
            Hasher.VerifyHashedPassword(new object(), passwordHash, password) != PasswordVerificationResult.Failed;

        public void VerifyDummy(string password) => Verify(DummyHash.Value, password);
    }
}
