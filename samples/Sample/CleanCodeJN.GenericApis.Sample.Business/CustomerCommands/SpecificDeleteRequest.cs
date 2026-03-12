using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Domain;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

/// <summary>
/// Represents a request to delete a specific customer entity identified by its unique ID.
/// </summary>
/// <remarks>This request is used to delete a customer and expects a response of type <see
/// cref="BaseResponse{T}"/> containing the deleted <see cref="Customer"/> entity or relevant status
/// information.</remarks>
public class SpecificDeleteRequest : IRequest<BaseResponse<Customer>>
{
    /// <summary>
    /// Gets the unique identifier of the customer to delete.
    /// </summary>
    public required int Id { get; init; }
}
