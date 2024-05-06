using CleanCodeJN.GenericApis.Contracts;

namespace CleanCodeJN.GenericApis.Commands;

public abstract class BaseIntegrationCommand(ICommandExecutionContext executionContext)
{
    public ICommandExecutionContext ExecutionContext { get; } = executionContext;
}
