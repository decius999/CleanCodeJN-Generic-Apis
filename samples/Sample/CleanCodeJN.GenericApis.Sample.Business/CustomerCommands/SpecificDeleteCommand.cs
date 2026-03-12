using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Domain;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

/// <summary>
/// Handles the deletion of a specific customer entity based on the provided request.
/// </summary>
/// <param name="repository">The customers repository. Implements <see cref="IRepository{TEntity, TKey}"/></param>
public class SpecificDeleteCommand(IRepository<Customer, int> repository) : IRequestHandler<SpecificDeleteRequest, BaseResponse<Customer>>
{
    /// <summary>
    /// Handles the <see cref="SpecificDeleteRequest"/> by deleting the customer from the repository.
    /// </summary>
    /// <param name="request">The request containing the ID of the customer to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="BaseResponse{T}"/> containing the deleted customer, or a failure result if not found.</returns>
    public async Task<BaseResponse<Customer>> Handle(SpecificDeleteRequest request, CancellationToken cancellationToken)
    {
        var deletedCustomer = await repository.Delete(request.Id, cancellationToken);

        return await BaseResponse<Customer>.Create(deletedCustomer is not null, deletedCustomer);
    }
}
