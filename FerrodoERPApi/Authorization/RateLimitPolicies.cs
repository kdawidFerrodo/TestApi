namespace FerrodoERPApi.Authorization
{
    public static class RateLimitPolicies
    {
        /// <summary>Ograniczenie prób logowania/odświeżania z jednego adresu IP (ochrona przed brute force).</summary>
        public const string Auth = "auth";
    }
}
