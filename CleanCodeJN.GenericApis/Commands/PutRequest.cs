using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class PutRequest<TEntity, TDto> : IRequest<BaseResponse<TEntity>>
    where TEntity : class
{
    public TDto Dto { get; init; }
}
