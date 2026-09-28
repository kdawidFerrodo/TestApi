namespace Infrastructure.Services.Auth
{
    public interface IPasswordHasher
    {
        string Hash(string password);

        bool Verify(string passwordHash, string password);

        /// <summary>
        /// Wykonuje weryfikację na fikcyjnym hashu, żeby logowanie na nieistniejący login trwało
        /// tyle samo co na istniejący (inaczej po czasie odpowiedzi da się zgadywać loginy).
        /// </summary>
        void VerifyDummy(string password);
    }
}
