using Microsoft.AspNetCore.Authorization;

namespace FerrodoERPApi.Authorization
{
    /// <summary>
    /// Wymaga, aby access token zawierał podane uprawnienie, np. [HasPermission(Permissions.Invoices.Read)].
    /// Kilka atrybutów na jednym endpoincie = wymagane wszystkie.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class HasPermissionAttribute : AuthorizeAttribute, IAuthorizationRequirementData
    {
        public HasPermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }

        public IEnumerable<IAuthorizationRequirement> GetRequirements()
        {
            yield return new PermissionRequirement(Permission);
        }
    }
}
