using AutoMapper;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class PutCommand<TEntity, TDto, TKey>(IMapper mapper, IRepository<TEntity, TKey> repository) : IRequestHandler<PutRequest<TEntity, TDto>, BaseResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    public async Task<BaseResponse<TEntity>> Handle(PutRequest<TEntity, TDto> request, CancellationToken cancellationToken)
    {
        var entity = await repository.Update(mapper.Map<TEntity>(request.Dto), cancellationToken);

        return await BaseResponse<TEntity>.Create(entity is not null, entity);
    }
}
