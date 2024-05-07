using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class GetCommand<TEntity, TKey>(IRepository<TEntity, TKey> repository) : IRequestHandler<GetRequest<TEntity, TKey>, BaseListResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    public async Task<BaseListResponse<TEntity>> Handle(GetRequest<TEntity, TKey> request, CancellationToken cancellationToken)
    {
        var query = repository.Query(request.Includes?.ToArray() ?? []);

        var count = query.Count();

        var entities = string.IsNullOrWhiteSpace(request.SortOrder) ?
            query.Where(request.Where).Skip(request.Skip).Take(request.Take).ToList() :
            query.Where(request.Where).OrderByString(request.SortField, request.SortOrder).Skip(request.Skip).Take(request.Take).ToList();

        return await BaseListResponse<TEntity>.Create(entities is not null, entities, count: count);
    }
}
