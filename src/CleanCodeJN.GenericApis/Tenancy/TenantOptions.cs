using Microsoft.AspNetCore.Http;

namespace CleanCodeJN.GenericApis.Tenancy;

/// <summary>
/// Configuration for multi-tenancy support.
/// </summary>
public class TenantOptions
{
    /// <summary>
    /// The JWT claim name used to extract the tenant identifier from the current user's claims.
    /// Used as the default resolver when <see cref="TenantResolver"/> is not set.
    /// Example: "tenant_id", "tid", "organization"
    /// </summary>
    public string ClaimName { get; set; }

    /// <summary>
    /// Optional custom resolver that extracts the tenant name from the HttpContext.
    /// Use this for non-claim scenarios such as subdomains, headers, or route values.
    /// When set, takes precedence over <see cref="ClaimName"/>.
    /// Example: ctx => ctx.Request.Headers["X-Tenant-ID"]
    /// </summary>
    public Func<HttpContext, string> TenantResolver { get; set; }

    /// <summary>
    /// Optional hook that maps a tenant name to a database connection string.
    /// When set, <see cref="TenantContext.ConnectionString"/> is populated automatically
    /// before any handler is invoked. Inject <see cref="TenantContext"/> in your DbContext
    /// to use the resolved connection string per request.
    /// Example: tenantName => configuration.GetConnectionString(tenantName)
    /// </summary>
    public Func<string, string> ConnectionStringResolver { get; set; }
}
