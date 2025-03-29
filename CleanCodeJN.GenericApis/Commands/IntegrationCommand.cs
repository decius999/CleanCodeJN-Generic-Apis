using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;
public abstract class IntegrationCommand<TRequest, TEntity>(ICommandExecutionContext executionContext) :
    BaseIntegrationCommand(executionContext), IRequestHandler<TRequest, BaseResponse<TEntity>>
    where TEntity : class, IEntity
    where TRequest : IRequest<BaseResponse<TEntity>>
{
    public abstract Task<BaseResponse<TEntity>> Handle(TRequest request, CancellationToken cancellationToken);
}
