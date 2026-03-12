using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Represents a patch request.
/// </summary>
/// <typeparam name="TEntity">TEntity type.</typeparam>
/// <typeparam name="TKey">Type of the Id column.</typeparam>
public class PatchRequest<TEntity, TKey> : IRequest<BaseResponse<TEntity>>
    where TEntity : class
{
    /// <summary>
    /// The Id to patch.
    /// </summary>
    public TKey Id { get; init; }

    /// <summary>
    /// Use the http context when calling directly from an API.
    /// </summary>
    public HttpContext HttpContext { get; set; }

    /// <summary>
    /// Use the PatchDocument directly when calling from somewhere else when the caller is not a http endpoint.
    /// </summary>
    public JsonPatchDocument<TEntity> PatchDocument { get; set; }
}
