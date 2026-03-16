using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Automatically registers a GraphQL mutation field for updating an entity of type <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TDto">The DTO type returned by the mutation.</typeparam>
/// <typeparam name="TEntity">The entity type to update.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
/// <typeparam name="TInput">The input type accepted by the mutation.</typeparam>
public class AutoUpdateMutationTypeExtensions<TDto, TEntity, TKey, TInput>(GraphQLOptions options, CleanCodeNamingConventions namingConventions = null) : ObjectTypeExtension
    where TEntity : class, IEntity<TKey>
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Mutation");

        var conventions = namingConventions ?? new CleanCodeNamingConventions();
        var field = descriptor
            .Field(conventions.GraphQLUpdatePrefix + typeof(TEntity).Name)
            .Argument("id", a => a.Type<NonNullType<IdType>>())
            .Argument("input", a => a.Type<NonNullType<InputObjectType<TInput>>>())
            .Type<ObjectType<TDto>>();

        if (options?.AddAuthorizationWithPolicyName is not null)
        {
            field.Authorize(options.AddAuthorizationWithPolicyName);
        }

        field.Resolve(async ctx =>
        {
            var repository = ctx.Service<IRepository<TEntity, TKey>>();
            var mapper = ctx.Service<ICleanCodeMapper>();
            var id = ctx.ArgumentValue<TKey>("id");
            var input = ctx.ArgumentValue<TInput>("input");

            var entity = repository.Query().FirstOrDefault(x => x.Id.Equals(id));
            if (entity == null)
            {
                throw new GraphQLException("Entity not found");
            }

            mapper.Map(input, entity);
            await repository.Update(entity, CancellationToken.None);

            return mapper.Map<TDto>(entity);
        });
    }
}

