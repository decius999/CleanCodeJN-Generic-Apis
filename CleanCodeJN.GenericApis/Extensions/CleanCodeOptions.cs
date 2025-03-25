using System.Reflection;
using AutoMapper;

namespace CleanCodeJN.GenericApis.Extensions;

public class CleanCodeOptions
{
    /// <summary>
    /// The assemblies that contain the command types.
    /// </summary>
    public List<Assembly> ApplicationAssemblies { get; set; } = [];

    /// <summary>
    /// The assembly that contains the validators types.
    /// </summary>
    public Assembly ValidatorAssembly { get; set; }

    /// <summary>
    /// The assembly that contains the mapping profiles.
    /// </summary>
    public Action<IMapperConfigurationExpression> MappingOverrides { get; set; }

    /// <summary>
    /// If true: Use distributed memory cache.
    /// </summary>
    public bool UseDistributedMemoryCache { get; set; } = true;
}
