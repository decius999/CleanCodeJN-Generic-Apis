using CleanCodeJN.GenericApis.Abstractions.Contracts;
using Mapster;

namespace CleanCodeJN.GenericApis.Mappers;

internal class MapsterAdapter(TypeAdapterConfig config) : ICleanCodeMapper
{
    public TDest Map<TDest>(object source)
        => (TDest)TypeAdapter.Adapt(source, source.GetType(), typeof(TDest), config);

    public TDest Map<TSrc, TDest>(TSrc source, TDest destination)
    {
        source.Adapt(destination, config);
        return destination;
    }

    public IQueryable<TDest> ProjectTo<TSrc, TDest>(IQueryable<TSrc> source)
        => source.ProjectToType<TDest>(config);
}
