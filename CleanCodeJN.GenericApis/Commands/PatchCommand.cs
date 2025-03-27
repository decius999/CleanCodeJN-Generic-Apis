using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace CleanCodeJN.GenericApis.Commands;

public class PatchCommand<TEntity, TKey>(IRepository<TEntity, TKey> repository) : IRequestHandler<PatchRequest<TEntity, TKey>, BaseResponse<TEntity>>
     where TEntity : class, IEntity<TKey>
{
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
