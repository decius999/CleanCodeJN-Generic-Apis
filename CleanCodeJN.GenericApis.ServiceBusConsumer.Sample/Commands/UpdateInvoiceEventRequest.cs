using System.Text.Json;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Commands;
public class UpdateInvoiceEventRequest(JsonElement root) : BaseEventRequest<UpdateInvoiceEventModel>(root), IRequest<Response>
{
}
