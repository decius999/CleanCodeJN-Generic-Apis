using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class DeleteRequest<TEntity> : IRequest<BaseResponse<TEntity>>
    where TEntity : class
{
    public int Id { get; init; }
}
