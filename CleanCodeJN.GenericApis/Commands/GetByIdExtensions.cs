using CleanCodeJN.GenericApis.Contracts;

namespace CleanCodeJN.GenericApis.Commands;
public static class GetByIdExtensions
{
    public static ICommandExecutionContext GetByIdRequest<TEntity, TKey>(this ICommandExecutionContext executionContext, TKey id, string requestName)
        where TEntity : class
        => executionContext
            .WithRequest(() => new GetByIdRequest<TEntity, TKey> { Id = id }, requestName);
}
