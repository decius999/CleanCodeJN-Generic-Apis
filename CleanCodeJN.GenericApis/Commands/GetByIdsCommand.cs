using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;
public class GetByIdsCommand<TEntity>(IIntRepository<TEntity> repository) : IRequestHandler<GetByIdsRequest<TEntity>, BaseListResponse<TEntity>>
    where TEntity : class, IEntity<int>
{
    public async Task<BaseListResponse<TEntity>> Handle(GetByIdsRequest<TEntity> request, CancellationToken cancellationToken)
    {
        var entities = repository
            .Query(request.Includes?.ToArray() ?? [])
            .Where(x => request.Ids.Contains(x.Id))
            .ToList();

        return await BaseListResponse<TEntity>.Create(entities?.Any() == true, entities);
    }
}
