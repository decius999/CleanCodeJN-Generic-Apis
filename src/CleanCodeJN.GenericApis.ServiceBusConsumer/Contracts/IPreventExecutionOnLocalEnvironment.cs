namespace CleanCodeJN.GenericApis.ServiceBusConsumer.Contracts;

/// <summary>
/// Marker interface that signals a MediatR request should be intercepted and skipped when running in a local debugging environment.
/// </summary>
public interface IPreventExecutionOnLocalEnvironment
{
}
