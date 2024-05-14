using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class GetCommand<TEntity, TKey>(IRepository<TEntity, TKey> repository) : IRequestHandler<GetRequest<TEntity, TKey>, BaseListResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    public async Task<BaseListResponse<TEntity>> Handle(GetRequest<TEntity, TKey> request, CancellationToken cancellationToken)
    {
        var query = repository
            .Query(request.Includes?.ToArray() ?? [])
            .Where(request.Where);

        var count = query.Count();

        var entities = query.PagedResultList<TEntity, TKey>(
                     request.Skip,
                     request.Take,
                     request.SortField,
                     request.SortOrder,
                     request.Filter);

        return await BaseListResponse<TEntity>.Create(entities is not null, entities.ToList(), count: count);
    }
}
