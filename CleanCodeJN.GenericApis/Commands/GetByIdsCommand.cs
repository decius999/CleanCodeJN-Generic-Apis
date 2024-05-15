using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;
public class GetByIdsCommand<TEntity, TKey>(IRepository<TEntity, TKey> repository) : IRequestHandler<GetByIdsRequest<TEntity, TKey>, BaseListResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    public async Task<BaseListResponse<TEntity>> Handle(GetByIdsRequest<TEntity, TKey> request, CancellationToken cancellationToken)
    {
        var entities = repository
            .Query(request.Includes?.ToArray() ?? [])
            .Where(x => request.Ids.Contains(x.Id))
            .ToList();

        return await BaseListResponse<TEntity>.Create(entities?.Any() == true, entities);
    }
}
