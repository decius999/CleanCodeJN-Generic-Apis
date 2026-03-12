using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// PutRequest
/// </summary>
/// <typeparam name="TEntity">TEntity type.</typeparam>
/// <typeparam name="TDto">Type of the Post DTO object.</typeparam>
public class PutRequest<TEntity, TDto> : IRequest<BaseResponse<TEntity>>
    where TEntity : class
{
    /// <summary>
    /// Put DTO object.
    /// </summary>
    public TDto Dto { get; init; }

    /// <summary>
    /// True if validation should be skipped.
    /// </summary>
    public bool SkipValidation { get; init; }
}
