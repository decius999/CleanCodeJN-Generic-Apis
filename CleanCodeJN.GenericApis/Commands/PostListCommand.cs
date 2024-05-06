using AutoMapper;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;
public class PostListCommand<TEntity, TDto>(IMapper mapper, IIntRepository<TEntity> repository) : IRequestHandler<PostListRequest<TEntity, TDto>, BaseListResponse<TEntity>>
    where TEntity : class, IEntity<int>
{
    public async Task<BaseListResponse<TEntity>> Handle(PostListRequest<TEntity, TDto> request, CancellationToken cancellationToken)
    {
        var entities = await repository.Create(request.Dtos.Select(dto => mapper.Map<TEntity>(dto)).ToList(), cancellationToken);

        return new BaseListResponse<TEntity>(entities is not null, entities.ToList());
    }
}
