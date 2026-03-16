using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Automatically registers a GraphQL mutation field for deleting an entity of type <typeparamref name="TEntity"/> by its ID.
/// </summary>
/// <typeparam name="TEntity">The entity type to delete.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public class AutoDeleteMutationTypeExtensions<TEntity, TKey>(GraphQLOptions options, CleanCodeNamingConventions namingConventions = null) : ObjectTypeExtension
    where TEntity : class, IEntity<TKey>
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Mutation");

        var conventions = namingConventions ?? new CleanCodeNamingConventions();
        var fieldName = conventions.GraphQLDeletePrefix + typeof(TEntity).Name;

        var field = descriptor
            .Field(fieldName)
            .Argument("id", a => a.Type<NonNullType<IdType>>())
            .Type<BooleanType>();

        if (options?.AddAuthorizationWithPolicyName is not null)
        {
            field.Authorize(options.AddAuthorizationWithPolicyName);
        }

        field.Resolve(async ctx =>
        {
            var id = ctx.ArgumentValue<TKey>("id");
            var repository = ctx.Service<IRepository<TEntity, TKey>>();

            var entity = await repository.Delete(id, CancellationToken.None);
            return entity != null;
        });
    }
}
