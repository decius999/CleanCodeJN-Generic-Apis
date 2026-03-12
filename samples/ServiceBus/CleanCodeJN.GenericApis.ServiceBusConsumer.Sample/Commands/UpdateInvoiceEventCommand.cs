using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Commands;

/// <summary>
/// Handles the <see cref="UpdateInvoiceEventRequest"/> by executing the customer retrieval and deletion pipeline.
/// </summary>
/// <param name="executionContext">The command execution context used to chain and execute requests.</param>
public class UpdateInvoiceEventCommand(ICommandExecutionContext executionContext) : BaseIntegrationCommand(executionContext), IRequestHandler<UpdateInvoiceEventRequest, Response>
{
    /// <summary>
    /// Handles the incoming invoice update event by retrieving the customer, fetching the related invoice, and deleting the customer.
    /// </summary>
    /// <param name="request">The event request containing the invoice update data.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Response"/> indicating the outcome of the pipeline execution.</returns>
    public async Task<Response> Handle(UpdateInvoiceEventRequest request, CancellationToken cancellationToken) =>
        await ExecutionContext
            .CustomerGetByIdRequest(request.Event.Data.CustomerId)
            .InvoiceGetFirstByIdRequest()
            .DeleteCustomerByIdRequest()
            .Execute(cancellationToken);
}
