namespace Domain.Authorization
{
    /// <summary>
    /// Dane użytkownika potrzebne do logowania i wystawienia tokenu.
    /// To projekcja z bazy (użytkownik + jego role + uprawnienia wynikające z ról),
    /// a nie encja - encję User możesz zaprojektować dowolnie.
    /// </summary>
    public sealed record UserAuthInfo(
        Guid Id,
        string UserName,
        string PasswordHash,
        bool IsActive,
        IReadOnlyCollection<string> Roles,
        IReadOnlyCollection<string> Permissions);
}
