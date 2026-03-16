using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using FluentValidation;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Handles the creation of a single entity from a DTO.
/// </summary>
/// <typeparam name="TEntity">The entity type to create.</typeparam>
/// <typeparam name="TDto">The DTO type used as input.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public class PostCommand<TEntity, TDto, TKey>(ICleanCodeMapper mapper, IRepository<TEntity, TKey> repository, IEnumerable<IValidator<TDto>> validators) : IRequestHandler<PostRequest<TEntity, TDto>, BaseResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Validates the DTO, maps it to an entity, and persists it to the repository.
    /// </summary>
    /// <param name="request">The post request containing the DTO to create.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="BaseResponse{TEntity}"/> containing the created entity.</returns>
    public async Task<BaseResponse<TEntity>> Handle(PostRequest<TEntity, TDto> request, CancellationToken cancellationToken)
    {
        var result = await ValidationExtensions.Validate<TEntity, TDto>(validators, request.Dto, request.SkipValidation);

        if (!result.Succeeded)
        {
            return result;
        }

        var entity = await repository.Create(mapper.Map<TEntity>(request.Dto), cancellationToken);

        return await BaseResponse<TEntity>.Create(entity is not null, entity);
    }
}
