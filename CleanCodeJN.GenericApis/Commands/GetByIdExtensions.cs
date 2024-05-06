using CleanCodeJN.GenericApis.Contracts;

namespace CleanCodeJN.GenericApis.Commands;
public static class GetByIdExtensions
{
    public static ICommandExecutionContext GetByIdRequest<TEntity>(this ICommandExecutionContext executionContext, int id, string requestName)
        where TEntity : class
        => executionContext
            .WithRequest(() => new GetByIdRequest<TEntity> { Id = id }, requestName);
}
