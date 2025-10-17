using AutoMapper;
using AutoMapper.QueryableExtensions;
using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Extensions;

public class AutoQueryTypeExtensions<TDto, TEntity, TKey>(GraphQLOptions options) : ObjectTypeExtension
     where TEntity : class, IEntity<TKey>
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Query");

        var field = descriptor
            .Field(typeof(TEntity).Name.ToLowerInvariant() + "s")
            .UseProjection()
            .UseFiltering()
            .UseSorting();

        if (options?.AddAuthorizationWithPolicyName is not null)
        {
            field.Authorize(options.AddAuthorizationWithPolicyName);
        }

        field.Resolve(ctx =>
        {
            var repo = (IRepository<TEntity, TKey>)ctx.Service(typeof(IRepository<TEntity, TKey>));
            var mapper = ctx.Service<IMapper>();
            return repo.Query().ProjectTo<TDto>(mapper.ConfigurationProvider);
        });
    }
}
