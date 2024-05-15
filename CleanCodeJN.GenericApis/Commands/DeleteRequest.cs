using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class DeleteRequest<TEntity, TKey> : IRequest<BaseResponse<TEntity>>
    where TEntity : class
{
    public TKey Id { get; init; }
}
