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
            .Field(typeof(TEntity).Name.ToLowerInvariant())
            .UseProjection()
            .UseFiltering()
            .UseSorting()
            .Argument("skip", a => a.Type<IntType>())
            .Argument("take", a => a.Type<IntType>());

        if (options?.AddAuthorizationWithPolicyName is not null)
        {
            field.Authorize(options.AddAuthorizationWithPolicyName);
        }

        field.Resolve(ctx =>
        {
            var repository = (IRepository<TEntity, TKey>)ctx.Service(typeof(IRepository<TEntity, TKey>));
            var mapper = ctx.Service<IMapper>();
            var skip = ctx.ArgumentValue<int?>("skip") ?? 0;
            var take = ctx.ArgumentValue<int?>("take") ?? 100;

            return repository.Query()
                .ProjectTo<TDto>(mapper.ConfigurationProvider)
                .Skip(skip)
                .Take(take);
        });
    }
}
