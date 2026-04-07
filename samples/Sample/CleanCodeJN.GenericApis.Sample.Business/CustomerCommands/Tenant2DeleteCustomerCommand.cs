using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Contracts;
using CleanCodeJN.GenericApis.Sample.Domain;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

/// <summary>
/// Tenant2-specific handler for DeleteCustomerIntegrationRequest.
/// Discovered automatically by the TenantCommandRegistry at startup via IMultiTenantHandler.
/// Only invoked when the configured claim resolves to "Tenant2".
/// </summary>
public class Tenant2DeleteCustomerCommand(ICommandExecutionContext executionContext)
    : IntegrationCommand<DeleteCustomerIntegrationRequest, Customer>(executionContext), IMultiTenantHandler
{
    public string TenantName => "Tenant2";

    public override async Task<BaseResponse<Customer>> Handle(DeleteCustomerIntegrationRequest request, CancellationToken cancellationToken) =>
        await ExecutionContext
            .LoadCustomersInParallelRequest(request.Id)
            .LoadInvoiceByIdRequest()
            .CustomerGetByIdRequest(request.Id)
            .InvoiceGetFirstByIdRequest()
            .DeleteCustomerByIdRequest()
            .Execute<Customer>(cancellationToken);
}
