using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace CleanCodeJN.GenericApis.Commands;

public class PatchRequest<TEntity, TKey> : IRequest<BaseResponse<TEntity>>
    where TEntity : class
{
    public TKey Id { get; init; }

    public HttpContext HttpContext { get; set; }

    public JsonPatchDocument<TEntity> PatchDocument { get; set; }
}
