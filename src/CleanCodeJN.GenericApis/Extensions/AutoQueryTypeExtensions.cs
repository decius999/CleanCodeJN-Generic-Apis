using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Automatically registers a GraphQL query field for retrieving entities of type <typeparamref name="TEntity"/> with sorting and pagination.
/// </summary>
/// <typeparam name="TDto">The DTO type projected and returned by the query.</typeparam>
/// <typeparam name="TEntity">The entity type to query.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
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
            var mapper = ctx.Service<ICleanCodeMapper>();

            var orders = ctx.ArgumentValue<IReadOnlyList<SortInput>>("order");
            var skip = ctx.ArgumentValue<int?>("skip") ?? 0;
            var take = ctx.ArgumentValue<int?>("take") ?? 100;

            var query = repository.Query();
            foreach (var order in orders)
            {
                query = query.OrderByString(order.Field, order.Direction == SortDirection.DESC);
            }

            return mapper.ProjectTo<TEntity, TDto>(query.Skip(skip).Take(take));
        });
    }
}

/// <summary>
/// Specifies the sort direction for a GraphQL query ordering argument.
/// </summary>
public enum SortDirection
{
    /// <summary>Ascending sort order.</summary>
    ASC,
    /// <summary>Descending sort order.</summary>
    DESC
}

/// <summary>
/// Represents a sort specification with a field name and direction used in GraphQL query ordering.
/// </summary>
public class SortInput
{
    /// <summary>
    /// Gets or sets the property name of the entity to sort by.
    /// </summary>
    public string Field { get; set; } = "";

    /// <summary>
    /// Gets or sets the sort direction for the field.
    /// </summary>
    public SortDirection Direction { get; set; } = SortDirection.ASC;
}

/// <summary>
/// A GraphQL input object type for specifying sort order on <typeparamref name="TEntity"/> queries.
/// </summary>
/// <typeparam name="TEntity">The entity type whose fields can be used for sorting.</typeparam>
public class CustomSortInputType<TEntity> : InputObjectType<SortInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<SortInput> descriptor)
    {
        descriptor.Name($"{typeof(TEntity).Name}SortInput");
        descriptor.Field(x => x.Field).Type<NonNullType<StringType>>();
        descriptor.Field(x => x.Direction).Type<NonNullType<EnumType<SortDirection>>>();
    }
}
