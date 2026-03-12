using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Handles partial updates to an entity using a JSON Patch document or an HTTP context body.
/// </summary>
/// <typeparam name="TEntity">The entity type to patch.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public class PatchCommand<TEntity, TKey>(IRepository<TEntity, TKey> repository) : IRequestHandler<PatchRequest<TEntity, TKey>, BaseResponse<TEntity>>
     where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Applies the patch document (from the request or parsed from the HTTP body) to the entity and saves the changes.
    /// </summary>
    /// <param name="request">The patch request containing the entity ID and either a patch document or HTTP context.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="BaseResponse{TEntity}"/> containing the patched entity.</returns>
    public async Task<BaseResponse<TEntity>> Handle(PatchRequest<TEntity, TKey> request, CancellationToken cancellationToken)
    {
        var patchDocument = request.PatchDocument;

        if (patchDocument is null)
        {
            using var reader = new StreamReader(request.HttpContext.Request.Body);
            var body = await reader.ReadToEndAsync();
            patchDocument = JsonConvert.DeserializeObject<JsonPatchDocument<TEntity>>(body);
        }

        var entity = repository.Query().First(x => x.Id.Equals(request.Id));
        patchDocument.ApplyTo(entity);
        await repository.Update(entity, cancellationToken);

        return await BaseResponse<TEntity>.Create(true, entity);
    }
}
