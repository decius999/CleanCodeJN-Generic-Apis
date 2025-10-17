using AutoMapper;
using CleanCodeJN.Repository.EntityFramework.Contracts;

namespace CleanCodeJN.GenericApis.Extensions;

public class AutoCreateMutationTypeExtensions<TDto, TEntity, TKey, TInput> : ObjectTypeExtension
    where TEntity : class, IEntity<TKey>
{
    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Mutation");

        descriptor
            .Field("create" + typeof(TEntity).Name)
            .Argument("input", a => a.Type<NonNullType<InputObjectType<TInput>>>())
            .Type<ObjectType<TDto>>()
            .Resolve(async ctx =>
            {
                var repository = ctx.Service<IRepository<TEntity, TKey>>();
                var mapper = ctx.Service<IMapper>();
                var input = ctx.ArgumentValue<TInput>("input");

                var entity = mapper.Map<TEntity>(input);
                await repository.Create(entity, CancellationToken.None);

                return mapper.Map<TDto>(entity);
            });
    }
}

