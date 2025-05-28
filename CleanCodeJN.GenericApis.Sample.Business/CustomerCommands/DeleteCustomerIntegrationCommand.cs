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
            .WithParallelWhenAllRequests(
                [
                    () => new GetByIdRequest<Customer, int>
                          {
                              Id = request.Id,
                          },
                    () => new GetByIdRequest<Invoice, Guid>
                          {
                              Id = Guid.NewGuid(), // This should be replaced with the actual logic to get the invoice ID related to the customer
                          },
                ], blockName: "Parallel Block")
            .WithRequest(
                () => new GetByIdRequest<Invoice, Guid>
                {
                    Id = executionContext.GetParallelWhenAllByIndex<Invoice>("Parallel Block", 1).Id,
                })
            .CustomerGetByIdRequest(request.Id)
            .InvoiceGetFirstByIdRequest()
            .DeleteCustomerByIdRequest()
            .Execute<Customer>(cancellationToken);
}
