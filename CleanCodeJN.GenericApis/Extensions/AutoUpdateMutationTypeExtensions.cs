using AutoMapper;
using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Extensions;

public class AutoUpdateMutationTypeExtensions<TDto, TEntity, TKey, TInput> : ObjectTypeExtension
    where TEntity : class, IEntity<TKey>
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Mutation");

        descriptor
            .Field("update" + typeof(TEntity).Name)
            .Argument("id", a => a.Type<NonNullType<IdType>>())
            .Argument("input", a => a.Type<NonNullType<InputObjectType<TInput>>>())
            .Type<ObjectType<TDto>>()
            .Resolve(async ctx =>
            {
                var repository = ctx.Service<IRepository<TEntity, TKey>>();
                var mapper = ctx.Service<IMapper>();
                var id = ctx.ArgumentValue<TKey>("id");
                var input = ctx.ArgumentValue<TInput>("input");

                var entity = repository.Query().First(x => x.Id.Equals(id));
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

