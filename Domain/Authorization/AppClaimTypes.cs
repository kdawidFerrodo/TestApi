namespace Domain.Authorization
{
    /// <summary>
    /// Nazwy claimów umieszczanych w access tokenie (JWT).
    /// Krótkie, standardowe nazwy JWT zamiast długich URI z System.Security.Claims.ClaimTypes.
    /// </summary>
    public static class AppClaimTypes
    {
        public const string Subject = "sub";
        public const string Name = "name";
        public const string Role = "role";
        public const string Permission = "permission";
        public const string TokenId = "jti";
    }
}
