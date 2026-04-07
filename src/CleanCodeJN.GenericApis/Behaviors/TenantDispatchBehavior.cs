using CleanCodeJN.GenericApis.Tenancy;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CleanCodeJN.GenericApis.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enables multi-tenancy support.
/// Runs before every handler and:
/// 1. Extracts the tenant name from the configured HTTP context claim.
/// 2. Resolves the tenant's connection string via TenantOptions.ConnectionStringResolver (if configured).
/// 3. Stores both in the scoped TenantContext.
/// 4. Looks for an IMultiTenantHandler registered for the current tenant and request type.
///    If found, that handler is invoked. Otherwise the default handler is called via next().
/// </summary>
public class TenantDispatchBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    TenantContext tenantContext,
    TenantCommandRegistry registry,
    IServiceProvider serviceProvider,
    IOptions<TenantOptions> tenantOptions) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var options = tenantOptions.Value;
        var tenant = options.TenantResolver is not null
            ? options.TenantResolver(httpContextAccessor.HttpContext)
            : httpContextAccessor.HttpContext?.User?.FindFirst(options.ClaimName)?.Value;

        tenantContext.TenantName = tenant;

        if (!string.IsNullOrEmpty(tenant))
        {
            if (options.ConnectionStringResolver is not null)
            {
                tenantContext.ConnectionString = options.ConnectionStringResolver(tenant);
            }

            var handlerType = registry.Resolve(tenant, typeof(TRequest));
            if (handlerType is not null)
            {
                var handler = (IRequestHandler<TRequest, TResponse>)serviceProvider.GetRequiredService(handlerType);
                return await handler.Handle(request, cancellationToken);
            }
        }

        return await next();
    }
}
