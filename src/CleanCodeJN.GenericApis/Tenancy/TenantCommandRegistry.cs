using System.Reflection;
using System.Runtime.CompilerServices;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Tenancy;

/// <summary>
/// Singleton registry built at startup. Scans all assemblies for IRequestHandler implementations
/// that also implement IMultiTenantHandler, reads their declared TenantName without calling
/// the constructor, and builds a (tenantName, requestType) → handlerType dispatch map.
/// </summary>
public class TenantCommandRegistry
{
    private readonly Dictionary<(string Tenant, Type RequestType), Type> _map = [];

    public void Scan(IEnumerable<Assembly> assemblies)
    {
        var handlerInterface = typeof(IRequestHandler<,>);
        var tenantInterface = typeof(IMultiTenantHandler);

        foreach (var type in assemblies.SelectMany(a => a.GetTypes()).Where(t => t.IsClass && !t.IsAbstract))
        {
            if (!tenantInterface.IsAssignableFrom(type))
            {
                continue;
            }

            var requestHandlerImpl = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);

            if (requestHandlerImpl is null)
            {
                continue;
            }

            // Create an uninitialized instance (no constructor called) to read the constant TenantName.
            // This is safe as long as TenantName is implemented as a simple constant return (=> "EnBW").
            var uninit = (IMultiTenantHandler)RuntimeHelpers.GetUninitializedObject(type);
            var tenantName = uninit.TenantName;

            if (string.IsNullOrEmpty(tenantName))
            {
                continue;
            }

            var requestType = requestHandlerImpl.GetGenericArguments()[0];
            _map[(tenantName, requestType)] = type;
        }
    }

    public Type Resolve(string tenantName, Type requestType) =>
        _map.TryGetValue((tenantName, requestType), out var handlerType) ? handlerType : null;

    public IEnumerable<Type> HandlerTypes => _map.Values.Distinct();
}
