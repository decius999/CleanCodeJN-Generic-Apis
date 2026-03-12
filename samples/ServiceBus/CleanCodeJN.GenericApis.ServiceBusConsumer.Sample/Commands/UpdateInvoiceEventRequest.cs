using System.Text.Json;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.ServiceBusConsumer.Commands;
using MediatR;

namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Sample.Commands;

/// <summary>
/// Represents a service bus event request for updating an invoice, parsed from a raw JSON message.
/// </summary>
/// <param name="root">The root <see cref="JsonElement"/> of the incoming service bus message.</param>
public class UpdateInvoiceEventRequest(JsonElement root) : BaseEventRequest<UpdateInvoiceEventModel>(root), IRequest<Response>
{
}
