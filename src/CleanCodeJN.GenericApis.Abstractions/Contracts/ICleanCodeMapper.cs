namespace CleanCodeJN.GenericApis.Abstractions.Contracts;

/// <summary>
/// Abstraction over object-to-object mapping. Enables swapping the mapping provider
/// (e.g. AutoMapper, Mapster) without changing consuming code.
/// </summary>
public interface ICleanCodeMapper
{
    /// <summary>
    /// Maps the <paramref name="source"/> object to a new instance of <typeparamref name="TDest"/>.
    /// </summary>
    TDest Map<TDest>(object source);

    /// <summary>
    /// Maps properties from <paramref name="source"/> into the existing <paramref name="destination"/> object.
    /// </summary>
    TDest Map<TSrc, TDest>(TSrc source, TDest destination);

    /// <summary>
    /// Projects a queryable sequence of <typeparamref name="TSrc"/> to <typeparamref name="TDest"/>
    /// using the configured mapping provider (used for EF Core server-side projection in GraphQL queries).
    /// </summary>
    IQueryable<TDest> ProjectTo<TSrc, TDest>(IQueryable<TSrc> source);
}
