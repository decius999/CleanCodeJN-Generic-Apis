using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Abstractions.Responses;

public abstract class BaseIntegrationCommand(ICommandExecutionContext executionContext)
{
    public ICommandExecutionContext ExecutionContext { get; } = executionContext;
}
