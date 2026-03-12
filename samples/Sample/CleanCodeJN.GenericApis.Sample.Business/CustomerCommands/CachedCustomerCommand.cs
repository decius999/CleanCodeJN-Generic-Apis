using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Domain;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

/// <summary>
/// Handles requests to retrieve a cached list of customers.
/// </summary>
/// <remarks>This class processes <see cref="CachedCustomerRequest"/> and returns a <see
/// cref="BaseListResponse{T}"/> containing the list of customers retrieved from the repository. The response is created
/// with a success status.</remarks>
/// <param name="repository">The repository used to query customer data. Must implement <see cref="IRepository{TEntity, TKey}"/> with <see
/// cref="Customer"/> as the entity type and <see cref="int"/> as the key type.</param>
public class CachedCustomerCommand(IRepository<Customer, int> repository) : IRequestHandler<CachedCustomerRequest, BaseListResponse<Customer>>
{
    /// <summary>
    /// Handles the <see cref="CachedCustomerRequest"/> by querying and returning all customers.
    /// </summary>
    /// <param name="request">The cacheable request for retrieving customers.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="BaseListResponse{T}"/> containing all customers.</returns>
    public async Task<BaseListResponse<Customer>> Handle(CachedCustomerRequest request, CancellationToken cancellationToken) =>
        await BaseListResponse<Customer>.Create(true, repository.Query().ToList());
}
