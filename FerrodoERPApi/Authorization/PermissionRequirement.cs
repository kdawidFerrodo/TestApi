using Microsoft.AspNetCore.Authorization;

namespace FerrodoERPApi.Authorization
{
    public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
}
