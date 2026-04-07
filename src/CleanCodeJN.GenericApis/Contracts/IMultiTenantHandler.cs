using CleanCodeJN.GenericApis.Tenancy;

namespace CleanCodeJN.GenericApis.Contracts;

/// <summary>
/// Implement this interface on any IRequestHandler to receive the current tenant name.
/// Inject <see cref="TenantContext"/> in your handler's constructor and return TenantContext.TenantName.
/// The value is populated by TenantDispatchBehavior before the handler is invoked.
/// </summary>
public interface IMultiTenantHandler
{
    /// <summary>
    /// The tenant name extracted from the configured HTTP context claim.
    /// </summary>
    string TenantName { get; }
}
