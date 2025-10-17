using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Extensions;

public class AutoDeleteMutationTypeExtensions<TEntity, TKey> : ObjectTypeExtension
    where TEntity : class, IEntity<TKey>
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Mutation");

        var fieldName = "delete" + typeof(TEntity).Name;

        descriptor
            .Field(fieldName)
            .Argument("id", a => a.Type<NonNullType<IdType>>())
            .Type<BooleanType>()
            .Resolve(async ctx =>
            {
                var id = ctx.ArgumentValue<TKey>("id");
                var repository = ctx.Service<IRepository<TEntity, TKey>>();

                var entity = await repository.Delete(id, CancellationToken.None);
                return entity != null;
            });
    }
}
