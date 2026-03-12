using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Domain;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

/// <summary>
/// Represents a request to delete a customer integration.
/// </summary>
/// <remarks>This request is used to remove a specific customer integration identified by its unique ID.</remarks>
public class DeleteCustomerIntegrationRequest : IRequest<BaseResponse<Customer>>
{
    public required int Id { get; init; }
}
