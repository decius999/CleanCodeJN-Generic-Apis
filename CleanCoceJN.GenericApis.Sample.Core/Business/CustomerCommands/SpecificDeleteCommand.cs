using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Models;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Core.Business.CustomerCommands;

public class SpecificDeleteCommand(IRepository<Customer, int> repository) : IRequestHandler<SpecificDeleteRequest, BaseResponse<Customer>>
{
    public async Task<BaseResponse<Customer>> Handle(SpecificDeleteRequest request, CancellationToken cancellationToken)
    {
        var deletedCustomer = await repository.Delete(request.Id, cancellationToken);

        return await BaseResponse<Customer>.Create(deletedCustomer is not null, deletedCustomer);
    }
}
