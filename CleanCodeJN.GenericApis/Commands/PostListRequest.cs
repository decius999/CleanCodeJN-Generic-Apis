using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// PostListRequest.
/// </summary>
/// <typeparam name="TEntity">TEntity type.</typeparam>
/// <typeparam name="TDto">Type of the Post DTO object.</typeparam>
public class PostListRequest<TEntity, TDto> : IRequest<BaseListResponse<TEntity>>
    where TEntity : class
{
    /// <summary>
    /// The DTOs to post.
    /// </summary>
    public List<TDto> Dtos { get; init; }

    /// <summary>
    /// True if´the validation should be skipped.
    /// </summary>
    public bool SkipValidation { get; init; }
}
