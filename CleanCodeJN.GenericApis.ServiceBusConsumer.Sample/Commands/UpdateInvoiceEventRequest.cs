using System.Text.Json;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Models;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Commands;
public class UpdateInvoiceEventRequest(JsonElement root) : BaseEventRequest<UpdateInvoiceEventModel>(root), IRequest<Response>
{
}
