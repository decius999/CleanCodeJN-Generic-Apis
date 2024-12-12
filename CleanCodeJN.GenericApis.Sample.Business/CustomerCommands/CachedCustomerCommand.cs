using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Domain;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;
public class CachedCustomerCommand(IRepository<Customer, int> repository) : IRequestHandler<CachedCustomerRequest, BaseListResponse<Customer>>
{
    public async Task<BaseListResponse<Customer>> Handle(CachedCustomerRequest request, CancellationToken cancellationToken) =>
        await BaseListResponse<Customer>.Create(true, repository.Query().ToList());
}
