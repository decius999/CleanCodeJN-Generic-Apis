using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Core.Dtos;
using CleanCodeJN.GenericApis.Sample.Domain;
using CleanCodeJN.GenericApis.SmartMapper;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;
public class CachedCustomerCommand(IRepository<Customer, int> repository, ISmartMapper mapper) : IRequestHandler<CachedCustomerRequest, BaseListResponse<Customer>>
{
    public async Task<BaseListResponse<Customer>> Handle(CachedCustomerRequest request, CancellationToken cancellationToken)
    {
        var customers = repository.Query([x => x.Invoices]).ToList();
        var customer = customers.First(x => x.Invoices.Any());
        var dto = mapper.Map<Customer, CustomerGetDto>(customer);

        customer.Name = dto.Name;

        return await BaseListResponse<Customer>.Create(true, customers);
    }
}
