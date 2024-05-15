using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;
public class PostListCommand<TEntity, TDto, TKey>(IMapper mapper, IRepository<TEntity, TKey> repository) : IRequestHandler<PostListRequest<TEntity, TDto>, BaseListResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    public async Task<BaseListResponse<TEntity>> Handle(PostListRequest<TEntity, TDto> request, CancellationToken cancellationToken)
    {
        var entities = await repository.Create(request.Dtos.Select(dto => mapper.Map<TEntity>(dto)).ToList(), cancellationToken);

        return await BaseListResponse<TEntity>.Create(entities is not null, entities.ToList());
    }
}
