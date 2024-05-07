using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class DeleteCommand<TEntity, TKey>(IRepository<TEntity, TKey> repository) : IRequestHandler<DeleteRequest<TEntity, TKey>, BaseResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    public async Task<BaseResponse<TEntity>> Handle(DeleteRequest<TEntity, TKey> request, CancellationToken cancellationToken)
    {
        var entity = await repository.Delete(request.Id, cancellationToken);

        return new BaseResponse<TEntity>(entity is not null, entity);
    }
}
