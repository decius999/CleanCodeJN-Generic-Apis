using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class DeleteCommand<TEntity>(IIntRepository<TEntity> repository) : IRequestHandler<DeleteRequest<TEntity>, BaseResponse<TEntity>>
    where TEntity : class, IEntity<int>
{
    public async Task<BaseResponse<TEntity>> Handle(DeleteRequest<TEntity> request, CancellationToken cancellationToken)
    {
        var entity = await repository.Delete(request.Id, cancellationToken);

        return new BaseResponse<TEntity>(entity is not null);
    }
}
