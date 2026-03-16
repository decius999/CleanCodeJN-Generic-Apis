using AutoMapper;
using AutoMapper.QueryableExtensions;
using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Mappers;

internal class AutoMapperAdapter(IMapper mapper) : ICleanCodeMapper
{
    public TDest Map<TDest>(object source) => mapper.Map<TDest>(source);

    public TDest Map<TSrc, TDest>(TSrc source, TDest destination) => mapper.Map(source, destination);

    public IQueryable<TDest> ProjectTo<TSrc, TDest>(IQueryable<TSrc> source)
        => source.ProjectTo<TDest>(mapper.ConfigurationProvider);
}
