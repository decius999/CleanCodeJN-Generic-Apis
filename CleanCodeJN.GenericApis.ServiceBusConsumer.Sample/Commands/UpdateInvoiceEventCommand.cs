using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Commands;
public class UpdateInvoiceEventCommand : IRequestHandler<UpdateInvoiceEventRequest, Response>
{
    public async Task<Response> Handle(UpdateInvoiceEventRequest request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        return new Response(ResultEnum.SUCCESS);
    }
}
