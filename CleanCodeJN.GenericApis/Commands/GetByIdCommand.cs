using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class GetByIdCommand<TEntity, TKey>(IRepository<TEntity, TKey> repository) : IRequestHandler<GetByIdRequest<TEntity, TKey>, BaseResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    public async Task<BaseResponse<TEntity>> Handle(GetByIdRequest<TEntity, TKey> request, CancellationToken cancellationToken)
    {
        var entity = repository
             .Query(
                asNoTracking: request.AsNoTracking,
                ignoreQueryFilters: request.IgnoreQueryFilters,
                asSplitQuery: request.AsSplitQuery,
                includes: request.Includes?.ToArray() ?? [])
            .Where(request.Where)
            .FirstOrDefault(x => x.Id.Equals(request.Id));

        return await BaseResponse<TEntity>.Create(
            entity is not null ? ResultEnum.SUCCESS : ResultEnum.FAILURE_NOT_FOUND,
            entity,
            message: entity is not null ? null : $"Id '{request.Id}' not found!");
    }
}
