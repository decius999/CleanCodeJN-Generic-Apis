using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Delete Request
/// </summary>
/// <typeparam name="TEntity">TEntity type.</typeparam>
/// <typeparam name="TKey">Type of the Id column.</typeparam>
public class DeleteRequest<TEntity, TKey> : IRequest<BaseResponse<TEntity>>
    where TEntity : class
{
    /// <summary>
    /// Id of the entity to delete.
    /// </summary>
    public TKey Id { get; init; }
}
