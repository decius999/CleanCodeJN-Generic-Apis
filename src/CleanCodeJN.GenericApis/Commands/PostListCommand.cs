using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using FluentValidation;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;
public class PostListCommand<TEntity, TDto, TKey>(IMapper mapper, IRepository<TEntity, TKey> repository, IEnumerable<IValidator<TDto>> validators) : IRequestHandler<PostListRequest<TEntity, TDto>, BaseListResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    public async Task<BaseListResponse<TEntity>> Handle(PostListRequest<TEntity, TDto> request, CancellationToken cancellationToken)
    {
        List<string> errorMessages = [];
        foreach (var dto in request.Dtos)
        {
            var result = await ValidationExtensions.Validate<TEntity, TDto>(validators, dto, request.SkipValidation);

            if (!result.Succeeded)
            {
                errorMessages.Add(result.Message);
            }
        }

        if (errorMessages.Any())
        {
            return await BaseListResponse<TEntity>.Create(false, message: string.Join(", ", errorMessages));
        }

        var entities = await repository.Create(request.Dtos.Select(dto => mapper.Map<TEntity>(dto)).ToList(), cancellationToken);

        return await BaseListResponse<TEntity>.Create(entities is not null, entities.ToList());
    }
}
