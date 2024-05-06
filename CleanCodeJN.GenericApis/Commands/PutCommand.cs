using AutoMapper;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class PutCommand<TEntity, TDto>(IMapper mapper, IIntRepository<TEntity> repository) : IRequestHandler<PutRequest<TEntity, TDto>, BaseResponse<TEntity>>
    where TEntity : class, IEntity<int>
{
    public async Task<BaseResponse<TEntity>> Handle(PutRequest<TEntity, TDto> request, CancellationToken cancellationToken)
    {
        var entity = await repository.Update(mapper.Map<TEntity>(request.Dto), cancellationToken);

        return new BaseResponse<TEntity>(entity is not null, entity);
    }
}
