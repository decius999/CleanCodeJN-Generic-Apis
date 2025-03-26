using System.Reflection;
using AutoMapper;

namespace CleanCodeJN.GenericApis.Extensions;

public class CleanCodeOptions
{
    /// <summary>
    /// The assemblies that contain the command types for automatic registration of commands, DTOs and entities.
    /// </summary>
    public List<Assembly> ApplicationAssemblies { get; set; } = [];

    /// <summary>
    /// The assembly that contains the validators types for using Fluent Validation.
    /// </summary>
    public Assembly ValidatorAssembly { get; set; }

    /// <summary>
    /// The assembly that contains the automapper mapping profiles.
    /// </summary>
    public Action<IMapperConfigurationExpression> MappingOverrides { get; set; }

    /// <summary>
    /// If true: Use distributed memory cache.
    /// </summary>
    public bool UseDistributedMemoryCache { get; set; } = true;
}
