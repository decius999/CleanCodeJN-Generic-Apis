using CleanCodeJN.GenericApis.Abstractions.Contracts;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Provides extension methods for building get-by-id requests on an <see cref="ICommandExecutionContext"/>.
/// </summary>
public static class GetByIdExtensions
{
    /// <summary>
    /// Adds a get-by-id request for the specified entity and key to the execution context.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to retrieve.</typeparam>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <param name="executionContext">The command execution context to chain the request onto.</param>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="requestName">The named block identifier used to retrieve the result from the context cache.</param>
    /// <returns>The updated <see cref="ICommandExecutionContext"/> for further chaining.</returns>
    public static ICommandExecutionContext GetByIdRequest<TEntity, TKey>(this ICommandExecutionContext executionContext, TKey id, string requestName)
        where TEntity : class
        => executionContext
            .WithRequest(() => new GetByIdRequest<TEntity, TKey> { Id = id }, requestName);
}
