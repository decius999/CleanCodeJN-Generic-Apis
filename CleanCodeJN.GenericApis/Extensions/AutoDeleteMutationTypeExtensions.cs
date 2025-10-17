using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Extensions;

public class AutoDeleteMutationTypeExtensions<TEntity, TKey>(GraphQLOptions options) : ObjectTypeExtension
    where TEntity : class, IEntity<TKey>
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Mutation");

        var fieldName = "delete" + typeof(TEntity).Name;

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
