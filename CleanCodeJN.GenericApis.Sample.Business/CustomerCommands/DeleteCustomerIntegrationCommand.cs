using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Sample.Domain;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

public class DeleteCustomerIntegrationCommand(ICommandExecutionContext executionContext)
    : IntegrationCommand<DeleteCustomerIntegrationRequest, Customer>(executionContext)
{
    public override async Task<BaseResponse<Customer>> Handle(DeleteCustomerIntegrationRequest request, CancellationToken cancellationToken) =>
        await ExecutionContext
            .CustomerGetByIdRequest(request.Id)
            .InvoiceGetFirstByIdRequest()
            .DeleteCustomerByIdRequest()
            .Execute<Customer>(cancellationToken);
}
