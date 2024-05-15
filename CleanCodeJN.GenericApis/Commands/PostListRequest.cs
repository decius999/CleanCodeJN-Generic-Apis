using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class PostListRequest<TEntity, TDto> : IRequest<BaseListResponse<TEntity>>
    where TEntity : class
{
    public List<TDto> Dtos { get; init; }
}
