using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using FluentValidation;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Handles the creation of multiple entities from a list of DTOs in a single batch operation.
/// </summary>
/// <typeparam name="TEntity">The entity type to create.</typeparam>
/// <typeparam name="TDto">The DTO type used as input for each entity.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public class PostListCommand<TEntity, TDto, TKey>(ICleanCodeMapper mapper, IRepository<TEntity, TKey> repository, IEnumerable<IValidator<TDto>> validators) : IRequestHandler<PostListRequest<TEntity, TDto>, BaseListResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Validates each DTO, maps them to entities, and persists them all to the repository.
    /// </summary>
    /// <param name="request">The post-list request containing the list of DTOs.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="BaseListResponse{TEntity}"/> containing the created entities.</returns>
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
