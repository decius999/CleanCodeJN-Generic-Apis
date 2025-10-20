using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Sample.Domain;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

/// <summary>
/// Handles the deletion of a customer integration by executing a series of related requests.
/// </summary>
/// <remarks>This command processes the deletion of a customer integration by performing the following steps: 1.
/// Retrieves the customer and associated invoice in parallel. 2. Executes additional requests to validate and process
/// the deletion.  The command ensures that all necessary dependencies are resolved and processed before the customer
/// integration is deleted.</remarks>
/// <param name="executionContext"></param>
public class DeleteCustomerIntegrationCommand(ICommandExecutionContext executionContext)
    : IntegrationCommand<DeleteCustomerIntegrationRequest, Customer>(executionContext)
{
    /// <inheritdoc/>
    public override async Task<BaseResponse<Customer>> Handle(DeleteCustomerIntegrationRequest request, CancellationToken cancellationToken) =>
        await ExecutionContext
            .LoadCustomersInParallelRequest(request.Id)
            .LoadInvoiceByIdRequest()
            .CustomerGetByIdRequest(request.Id)
            .InvoiceGetFirstByIdRequest()
            .DeleteCustomerByIdRequest()
            .Execute<Customer>(cancellationToken);
}
