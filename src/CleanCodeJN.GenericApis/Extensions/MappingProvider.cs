namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Specifies which mapping library is used internally by CleanCodeJN.
/// </summary>
public enum MappingProvider
{
    /// <summary>
    /// Use AutoMapper (default). Configure overrides via <see cref="CleanCodeOptions.MappingOverrides"/>.
    /// </summary>
    AutoMapper,

    /// <summary>
    /// Use Mapster. Configure overrides via <see cref="CleanCodeOptions.MapsterMappingOverrides"/>.
    /// </summary>
    Mapster,
}
