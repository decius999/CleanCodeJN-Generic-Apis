using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// The base class for integration commands
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <param name="executionContext">The ICommandExecutionContext instance</param>
public abstract class IntegrationCommand<TRequest, TEntity>(ICommandExecutionContext executionContext) :
    BaseIntegrationCommand(executionContext), IRequestHandler<TRequest, BaseResponse<TEntity>>
    where TEntity : class, IEntity
    where TRequest : IRequest<BaseResponse<TEntity>>
{
    /// <summary>
    /// The handle method for the command
    /// </summary>
    /// <param name="request">The TRequest instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>BaseResponse of TEntity.</returns>
    public abstract Task<BaseResponse<TEntity>> Handle(TRequest request, CancellationToken cancellationToken);
}
