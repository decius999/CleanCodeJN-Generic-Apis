using System.Reflection;
using AutoMapper;
using CleanCodeJN.GenericApis.Tenancy;
using Mapster;

namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// The options for the CleanCodeJN.GenericApis
/// </summary>
public class CleanCodeOptions
{
    /// <summary>
    /// The assemblies that contain the command types, Entity types and DTO types for automatic registration of commands, DTOs and entities.
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
    /// If true: Use distributed memory cache. If false: you can add another Distributed Cache implementation.
    /// </summary>
    public bool UseDistributedMemoryCache { get; set; } = true;

    /// <summary>
    /// If true: Add default logging behavior. If false: you can add another logging behavior.
    /// </summary>
    public bool AddDefaultLoggingBehavior { get; set; }

    /// <summary>
    /// Mediatr Types of Open Behaviors to register
    /// </summary>
    public List<Type> OpenBehaviors { get; set; } = [];

    /// <summary>
    /// Mediatr Types of Closed Behaviors to register
    /// </summary>
    public List<Type> ClosedBehaviors { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether GraphQL auto-wiring is enabled.
    /// </summary>
    public GraphQLOptions GraphQLOptions { get; set; }

    /// <summary>
    /// Options for AI Proxy
    /// </summary>
    public AiProxyOptions AiProxyOptions { get; set; }

    /// <summary>
    /// Naming conventions for DTO discovery and GraphQL field name generation.
    /// Override defaults when your project uses different suffixes or prefixes.
    /// </summary>
    public CleanCodeNamingConventions NamingConventions { get; set; } = new();

    /// <summary>
    /// Selects the object mapping provider. Default is <see cref="MappingProvider.AutoMapper"/>.
    /// </summary>
    public MappingProvider MappingProvider { get; set; } = MappingProvider.AutoMapper;

    /// <summary>
    /// Optional Mapster-specific mapping overrides. Only used when <see cref="MappingProvider"/> is <see cref="MappingProvider.Mapster"/>.
    /// </summary>
    public Action<TypeAdapterConfig> MapsterMappingOverrides { get; set; }

    /// <summary>
    /// Optional multi-tenancy configuration. When set, activates the tenant dispatch
    /// pipeline: the configured claim is read from the HTTP context on every request,
    /// handlers tagged with <c>[TenantCommand("name")]</c> are routed automatically,
    /// and <c>TenantContext.TenantName</c> is available to all handlers in scope.
    /// </summary>
    public TenantOptions TenantOptions { get; set; }
}
