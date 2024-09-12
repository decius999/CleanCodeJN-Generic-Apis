using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Domain;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

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
