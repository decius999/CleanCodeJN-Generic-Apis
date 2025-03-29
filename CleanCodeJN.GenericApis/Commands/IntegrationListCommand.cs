using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;
public abstract class IntegrationListCommand<TRequest, TEntity>(ICommandExecutionContext executionContext) :
    BaseIntegrationCommand(executionContext), IRequestHandler<TRequest, BaseListResponse<TEntity>>
    where TEntity : class
    where TRequest : IRequest<BaseListResponse<TEntity>>
{
    public abstract Task<BaseListResponse<TEntity>> Handle(TRequest request, CancellationToken cancellationToken);
}
