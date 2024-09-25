using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Commands;
public class UpdateInvoiceEventCommand(ICommandExecutionContext executionContext) : BaseIntegrationCommand(executionContext), IRequestHandler<UpdateInvoiceEventRequest, Response>
{
    public async Task<Response> Handle(UpdateInvoiceEventRequest request, CancellationToken cancellationToken) =>
        await ExecutionContext
            .CustomerGetByIdRequest(request.Event.Data.CustomerId)
            .InvoiceGetFirstByIdRequest()
            .DeleteCustomerByIdRequest()
            .Execute(cancellationToken);
}
