using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Base class for integration list commands
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <param name="executionContext">The ICommandExecutionContext instance</param>
public abstract class IntegrationListCommand<TRequest, TEntity>(ICommandExecutionContext executionContext) :
    BaseIntegrationCommand(executionContext), IRequestHandler<TRequest, BaseListResponse<TEntity>>
    where TEntity : class
    where TRequest : IRequest<BaseListResponse<TEntity>>
{
    /// <summary>
    ///  The handle method for the command
    /// </summary>
    /// <param name="request">The TRequest instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>BaseListResponse of TEntity.</returns>
    public abstract Task<BaseListResponse<TEntity>> Handle(TRequest request, CancellationToken cancellationToken);
}
