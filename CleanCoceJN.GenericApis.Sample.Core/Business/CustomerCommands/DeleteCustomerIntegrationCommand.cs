using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Models;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Core.Business.CustomerCommands;

public class DeleteCustomerIntegrationCommand(ICommandExecutionContext executionContext)
    : BaseIntegrationCommand(executionContext), IRequestHandler<DeleteCustomerIntegrationRequest, BaseResponse<Customer>>
{
    public async Task<BaseResponse<Customer>> Handle(DeleteCustomerIntegrationRequest request, CancellationToken cancellationToken) =>
        await ExecutionContext
            .CustomerGetByIdRequest(request.Id)
            .InvoiceGetFirstByIdRequest()
            .DeleteCustomerByIdRequest()
            .Execute<Customer>(cancellationToken);
}
