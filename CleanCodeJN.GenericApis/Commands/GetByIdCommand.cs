using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class GetByIdCommand<TEntity>(IIntRepository<TEntity> repository) : IRequestHandler<GetByIdRequest<TEntity>, BaseResponse<TEntity>>
    where TEntity : class, IEntity<int>
{
    public async Task<BaseResponse<TEntity>> Handle(GetByIdRequest<TEntity> request, CancellationToken cancellationToken)
    {
        var entity = repository
            .Query(request.Includes?.ToArray() ?? [])
            .Where(request.Where)
            .FirstOrDefault(x => x.Id == request.Id);

        return await BaseResponse<TEntity>.Create(entity is not null, entity);
    }
}
