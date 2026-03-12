using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Abstractions.Responses;

/// <summary>
/// Abstract base class for integration commands that carry an <see cref="ICommandExecutionContext"/> for IOSP-based request orchestration.
/// </summary>
public abstract class BaseIntegrationCommand(ICommandExecutionContext executionContext)
{
    /// <summary>
    /// Gets the execution context used to pipeline and execute integration requests.
    /// </summary>
    public ICommandExecutionContext ExecutionContext { get; } = executionContext;
}
