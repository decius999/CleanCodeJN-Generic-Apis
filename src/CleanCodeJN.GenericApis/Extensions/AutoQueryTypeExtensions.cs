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

        // ── list field ────────────────────────────────────────────────────────
        // Middleware order (outermost → innermost → resolver):
        //   UseProjection → ApplyPaging → UseFiltering → Resolver
        //
        // Execution order is reversed (innermost first):
        //   Resolver returns IQueryable<TDto> (no skip/take)
        //   → UseFiltering applies WHERE on the full IQueryable
        //   → ApplyPaging applies skip/take on the already-filtered IQueryable
        //   → UseProjection selects only requested GraphQL fields
        //
        // This ensures filtering always runs against the complete dataset, not
        // just the current page.
        var field = descriptor
            .Field(typeof(TEntity).Name.ToLowerInvariant())
            .UseProjection()
            .Use(next => async ctx =>
            {
                await next(ctx);
                if (ctx.Result is IQueryable<TDto> q)
                {
                    var skip = ctx.ArgumentValue<int?>("skip") ?? 0;
                    var take = ctx.ArgumentValue<int?>("take") ?? 100;
                    ctx.Result = q.Skip(skip).Take(take);
                }
            })
            .UseFiltering()
            .Argument("order", a => a.Type<ListType<NonNullType<CustomSortInputType<TEntity>>>>())
            .Argument("skip", a => a.Type<IntType>())
            .Argument("take", a => a.Type<IntType>());

        if (options?.AddAuthorizationWithPolicyName is not null)
            field.Authorize(options.AddAuthorizationWithPolicyName);

        field.Resolve(ctx =>
        {
            var repository = (IRepository<TEntity, TKey>)ctx.Service(typeof(IRepository<TEntity, TKey>));
            var mapper = ctx.Service<ICleanCodeMapper>();

            var orders = ctx.ArgumentValue<IReadOnlyList<SortInput>>("order");

            var query = repository.Query();
            foreach (var order in orders ?? [])
                query = query.OrderByString(order.Field, order.Direction == SortDirection.DESC);

            // skip/take NOT applied here — handled by the ApplyPaging middleware
            // after UseFiltering has narrowed down the full result set.
            return mapper.ProjectTo<TEntity, TDto>(query);
        });

        // ── count field ───────────────────────────────────────────────────────
        // The custom middleware (outermost) runs AFTER UseFiltering has applied
        // the where-clause to the IQueryable, then replaces the result with the count.
        //
        // Filtering happens on the projected DTO, exactly as it does for the list.
        // Counting the entity instead would give the two fields different filter
        // input types, and a where-clause naming a DTO-only property — one that is
        // computed from a navigation — would be accepted by the list and rejected
        // here, taking the whole request down with it.
        var countField = descriptor
            .Field($"{typeof(TEntity).Name.ToLowerInvariant()}Count")
            .Type<NonNullType<IntType>>()
            .Use(next => async ctx =>
            {
                await next(ctx);
                if (ctx.Result is IQueryable<TDto> q)
                    ctx.Result = q.Count();
            })
            .UseFiltering<TDto>();

        if (options?.AddAuthorizationWithPolicyName is not null)
            countField.Authorize(options.AddAuthorizationWithPolicyName);

        countField.Resolve(ctx =>
        {
            var repository = (IRepository<TEntity, TKey>)ctx.Service(typeof(IRepository<TEntity, TKey>));
            var mapper = ctx.Service<ICleanCodeMapper>();

            return mapper.ProjectTo<TEntity, TDto>(repository.Query());
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
