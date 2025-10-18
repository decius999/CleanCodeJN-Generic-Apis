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
            .Argument("order", a => a.Type<ListType<NonNullType<CustomSortInputType<TEntity>>>>())
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

            var orders = ctx.ArgumentValue<IReadOnlyList<SortInput>>("order");
            var skip = ctx.ArgumentValue<int?>("skip") ?? 0;
            var take = ctx.ArgumentValue<int?>("take") ?? 100;

            var query = repository.Query();
            foreach (var order in orders)
            {
                query = query.OrderByString(order.Field, order.Direction == SortDirection.DESC);
            }

            return query
                    .Skip(skip)
                    .Take(take)
                    .ProjectTo<TDto>(mapper.ConfigurationProvider);
        });
    }
}

public enum SortDirection
{
    ASC,
    DESC
}

public class SortInput
{
    public string Field { get; set; } = "";
    public SortDirection Direction { get; set; } = SortDirection.ASC;
}

public class CustomSortInputType<TEntity> : InputObjectType<SortInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<SortInput> descriptor)
    {
        descriptor.Name($"{typeof(TEntity).Name}SortInput");
        descriptor.Field(x => x.Field).Type<NonNullType<StringType>>();
        descriptor.Field(x => x.Direction).Type<NonNullType<EnumType<SortDirection>>>();
    }
}
