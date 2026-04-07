namespace CleanCodeJN.GenericApis.Tenancy;

/// <summary>
/// Scoped service that holds the resolved tenant name and connection string for the current request.
/// Populated by TenantDispatchBehavior before any handler is invoked.
/// Inject this into your DbContext to route database access per tenant.
/// Implement IMultiTenantHandler on your request handlers to access TenantName.
/// </summary>
public class TenantContext
{
    /// <summary>
    /// The tenant name extracted from the configured HTTP context claim.
    /// </summary>
    public string TenantName { get; set; }

    /// <summary>
    /// The connection string resolved for the current tenant via TenantOptions.ConnectionStringResolver.
    /// Null when no resolver is configured.
    /// </summary>
    public string ConnectionString { get; set; }
}
