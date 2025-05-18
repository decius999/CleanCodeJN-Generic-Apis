using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Abstractions.Contracts;

/// <summary>
/// The Integration Operation Segregation Principle (IOSP) Execution Context. Used to seperate the execution of integration and operation request handler.
/// </summary>
/// <param name="commandBus">The mediatr service.</param>
public interface ICommandExecutionContext
{
    /// <summary>
    /// Get the response from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the object in the cache.</typeparam>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <returns>Type T.</returns>
    T Get<T>(string blockName) where T : class;

    /// <summary>
    /// Get the list from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the object in the cache.</typeparam>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <returns>List of type T.</returns>
    List<T> GetList<T>(string blockName);

    /// <summary>
    /// Execute the commands in the context with BaseResponse.
    /// </summary>
    /// <typeparam name="T">The IRequest of T type.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>BaseResponse of T.</returns>
    Task<BaseResponse<T>> Execute<T>(CancellationToken cancellationToken) where T : class;

    /// <summary>
    /// Execute the commands in the context with BaseListResponse.
    /// </summary>
    /// <typeparam name="T">The IRequest of T type.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>BaseListResponse of T.</returns>
    Task<BaseListResponse<T>> ExecuteList<T>(CancellationToken cancellationToken);

    /// <summary>
    /// Execute the commands in the context with Response.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Response type for executing asynchronous events without returning data.</returns>
    Task<Response> Execute(CancellationToken cancellationToken);

    /// <summary>
    /// Add a request to the execution context with BaseListResponse.
    /// </summary>
    /// <typeparam name="T">The IRequest of T type.</typeparam>
    /// <param name="requestBuilder">Lamda for constructing the IRequest of T type.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <param name="checkBeforeExecution">Lamda to check if this block should be executed.</param>
    /// <param name="checkAfterExecution">Lamda to check if after the execution the next block should be executed.</param>
    /// <param name="continueOnCheckError">True: Continue executing. False: Stop executing of blocks on errors.</param>
    /// <returns>ICommandExecutionContext for pipelining more blocks.</returns>
    ICommandExecutionContext WithRequest<T>(Func<IRequest<BaseListResponse<T>>> requestBuilder, string blockName = null, Func<bool> checkBeforeExecution = null, Func<BaseListResponse<T>, bool> checkAfterExecution = null, bool? continueOnCheckError = false);

    /// <summary>
    /// Add a request to the execution context with BaseResponse.
    /// </summary>
    /// <typeparam name="T">The IRequest of T type.</typeparam>
    /// <param name="requestBuilder">Lamda for constructing the IRequest of T type.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <param name="checkBeforeExecution">Lamda to check if this block should be executed.</param>
    /// <param name="checkAfterExecution">Lamda to check if after the execution the next block should be executed.</param>
    /// <param name="continueOnCheckError">True: Continue executing. False: Stop executing of blocks on errors.</param>
    /// <returns>ICommandExecutionContext for pipelining more blocks.</returns>
    ICommandExecutionContext WithRequest<T>(Func<IRequest<BaseResponse<T>>> requestBuilder, string blockName = null, Func<bool> checkBeforeExecution = null, Func<BaseResponse<T>, bool> checkAfterExecution = null, bool? continueOnCheckError = false)
        where T : class;

    /// <summary>
    /// Add requests to the execution Context to execute in parallel
    /// </summary>
    /// <typeparam name="T">The IRequest of T type.</typeparam>
    /// <param name="requestBuilder">Lamda for constructing the IRequest of T type.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <param name="checkBeforeExecution">Lamda to check if this block should be executed.</param>
    /// <param name="checkAfterExecution">Lamda to check if after the execution the next block should be executed.</param>
    /// <param name="continueOnCheckError">True: Continue executing. False: Stop executing of blocks on errors.</param>
    /// <returns>ICommandExecutionContext for pipelining more blocks.</returns>
    ICommandExecutionContext WithParallelWhenAllRequests<T>(List<Func<IRequest<BaseResponse<T>>>> requestBuilder, string blockName = null, Func<bool> checkBeforeExecution = null, Func<BaseResponse<T>, bool> checkAfterExecution = null, bool? continueOnCheckError = false)
       where T : class;

    /// <summary>
    /// Add a request to the execution context with List of IRequest of Response.
    /// </summary>
    /// <param name="requestBuilder">Lamda for constructing the IRequest of T type.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <param name="continueOnCheckError">True: Continue executing. False: Stop executing of blocks on errors.</param>
    /// <returns>ICommandExecutionContext for pipelining more blocks.</returns>
    ICommandExecutionContext WithRequests(Func<List<IRequest<Response>>> requestBuilder, string blockName = null, bool? continueOnCheckError = false);

    /// <summary>
    /// Add a request to the execution context with Response.
    /// </summary>
    /// <param name="requestBuilder">Lamda for constructing the IRequest of T type.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <param name="checkBeforeExecution">Lamda to check if this block should be executed.</param>
    /// <param name="checkAfterExecution">Lamda to check if after the execution the next block should be executed.</param>
    /// <param name="continueOnCheckError">True: Continue executing. False: Stop executing of blocks on errors.</param>
    /// <returns>ICommandExecutionContext for pipelining more blocks.</returns>
    ICommandExecutionContext WithRequest(Func<IRequest<Response>> requestBuilder, string blockName = null, Func<bool> checkBeforeExecution = null, Func<Response, bool> checkAfterExecution = null, bool? continueOnCheckError = false);
}
