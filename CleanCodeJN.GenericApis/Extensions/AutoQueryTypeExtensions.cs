using AutoMapper;
using AutoMapper.QueryableExtensions;
using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Extensions;

public class AutoQueryTypeExtensions<TDto, TEntity, TKey> : ObjectTypeExtension
     where TEntity : class, IEntity<TKey>
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Query");

        descriptor
            .Field(typeof(TEntity).Name.ToLowerInvariant() + "s")
            .UseProjection()
            .UseFiltering()
            .UseSorting()
            .Resolve(ctx =>
            {
                var repo = (IRepository<TEntity, TKey>)ctx.Service(typeof(IRepository<TEntity, TKey>));
                var mapper = ctx.Service<IMapper>();
                return repo.Query().ProjectTo<TDto>(mapper.ConfigurationProvider);
            });
    }
}
