using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Abstractions.Contracts;

/// <summary>
/// The Integration Operation Segregation Principle (IOSP) Execution Context. Used to seperate the execution of integration and operation request handler.
/// </summary>
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
    /// Get the result list from the <see cref="WithParallelWhenAllRequests"/>.
    /// </summary>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <returns>List of result objects.</returns>
    List<object> GetListParallelWhenAll(string blockName);

    /// <summary>
    /// Retrieves an item of type <typeparamref name="T"/> from a parallel processing block by its name and index.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve. Must be a reference type.</typeparam>
    /// <param name="blockName">The name of the parallel processing block. Cannot be null or empty.</param>
    /// <param name="index">The zero-based index of the item within the block. Must be a non-negative integer.</param>
    /// <returns>The item of type <typeparamref name="T"/> at the specified index within the block,  or <see langword="null"/> if
    /// the item does not exist or the index is out of range.</returns>
    T GetParallelWhenAllByIndex<T>(string blockName, int index) where T : class;

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
    /// <param name="requestBuilder">Lamda for constructing the IRequest of T type.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <param name="checkBeforeExecution">Lamda to check if this block should be executed.</param>
    /// <param name="checkAfterExecution">Lamda to check if after the execution the next block should be executed.</param>
    /// <param name="continueOnCheckError">True: Continue executing. False: Stop executing of blocks on errors.</param>
    /// <returns>ICommandExecutionContext for pipelining more blocks.</returns>
    ICommandExecutionContext WithParallelWhenAllRequests(List<Func<IRequest<Response>>> requestBuilder, string blockName = null, Func<bool> checkBeforeExecution = null, Func<BaseResponse<Response>, bool> checkAfterExecution = null, bool? continueOnCheckError = false);

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

    /// <summary>
    /// Executes a request conditionally based on the provided predicates.
    /// </summary>
    /// <remarks>This method allows conditional execution of a request based on pre- and post-execution
    /// predicates.  If the <paramref name="ifBeforePredicate"/> returns <see langword="false"/>, the request is not
    /// executed.  If the <paramref name="ifAfterPredicate"/> is provided, it is evaluated after the request execution
    /// to validate the result.</remarks>
    /// <typeparam name="T">The type of the entity returned by the request.</typeparam>
    /// <param name="requestBuilder">A function that builds the request to be executed. Must not be <see langword="null"/>.</param>
    /// <param name="ifBeforePredicate">An optional predicate that determines whether the request should be executed.  If <see langword="null"/>, the
    /// request is always executed.</param>
    /// <param name="ifAfterPredicate">An optional predicate that determines whether the result of the request satisfies a condition.  If <see
    /// langword="null"/>, the result is not checked after execution.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <returns>The <see cref="ICommandExecutionContext"/> representing the execution context of the request.</returns>
    ICommandExecutionContext IfRequest<T>(
       Func<IRequest<BaseResponse<T>>> requestBuilder,
       Func<bool> ifBeforePredicate = null,
       Func<BaseResponse<T>, bool> ifAfterPredicate = null,
       string blockName = null) where T : class =>
           WithRequest(requestBuilder, checkAfterExecution: ifAfterPredicate, checkBeforeExecution: ifBeforePredicate, blockName: blockName, continueOnCheckError: true);

    /// <summary>
    /// Executes a request conditionally based on specified predicates before and after execution. If false, break the whole execution.
    /// </summary>
    /// <typeparam name="T">The type of the response data returned by the request.</typeparam>
    /// <param name="requestBuilder">A function that builds the request to be executed.</param>
    /// <param name="ifBeforePredicate">An optional predicate to evaluate before executing the request.  If the predicate returns <see
    /// langword="false"/>, the request will not be executed.</param>
    /// <param name="ifAfterPredicate">An optional predicate to evaluate after executing the request.  If the predicate returns <see
    /// langword="false"/>, the execution context will not proceed further.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <returns>An <see cref="ICommandExecutionContext"/> representing the execution context of the request.</returns>
    ICommandExecutionContext IfBreakRequest<T>(
      Func<IRequest<BaseResponse<T>>> requestBuilder,
      Func<bool> ifBeforePredicate = null,
      Func<BaseResponse<T>, bool> ifAfterPredicate = null,
      string blockName = null) where T : class =>
          WithRequest(requestBuilder, checkAfterExecution: ifAfterPredicate, checkBeforeExecution: ifBeforePredicate, blockName: blockName, continueOnCheckError: false);

    /// <summary>
    /// Executes a request conditionally based on the provided predicates.
    /// </summary>
    /// <remarks>This method is designed to provide conditional execution of a request, allowing both
    /// pre-execution and post-execution checks through the provided predicates. The request will only proceed if the
    /// <paramref name="ifBeforePredicate"/> (if provided) evaluates to <see langword="true"/>. After execution, the
    /// <paramref name="ifAfterPredicate"/> (if provided) can be used to validate the response.</remarks>
    /// <typeparam name="T">The type of the entity contained in the request's response.</typeparam>
    /// <param name="requestBuilder">A function that builds the request to be executed. The function must return an <see cref="IRequest{TResponse}"/>
    /// where the response is of type <see cref="BaseListResponse{TEntity}"/>.</param>
    /// <param name="ifBeforePredicate">An optional predicate that determines whether the request should be executed. If the predicate returns <see
    /// langword="false"/>, the request will not be executed. Defaults to <see langword="null"/>, which means the
    /// request will always be executed unless another condition prevents it.</param>
    /// <param name="ifAfterPredicate">An optional predicate that evaluates the response after the request is executed. If the predicate returns <see
    /// langword="false"/>, subsequent operations may be affected based on the implementation. Defaults to <see
    /// langword="null"/>, which means no post-execution check is performed.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <returns>An <see cref="ICommandExecutionContext"/> that represents the execution context of the command, allowing further
    /// chaining or inspection of the operation.</returns>
    ICommandExecutionContext IfRequest<T>(
        Func<IRequest<BaseListResponse<T>>> requestBuilder,
        Func<bool> ifBeforePredicate = null,
        Func<BaseListResponse<T>, bool> ifAfterPredicate = null,
        string blockName = null) where T : class =>
            WithRequest(requestBuilder, checkAfterExecution: ifAfterPredicate, checkBeforeExecution: ifBeforePredicate, blockName: blockName, continueOnCheckError: true);

    /// <summary>
    /// Executes a request conditionally based on specified predicates. If false, break the whole execution.
    /// </summary>
    /// <remarks>This method allows conditional execution of a request based on pre- and post-execution
    /// predicates. The <paramref name="ifBeforePredicate"/> is evaluated before the request is executed, and the
    /// <paramref name="ifAfterPredicate"/> is evaluated after the request completes. If either predicate indicates a
    /// break condition, the execution context will reflect this.</remarks>
    /// <typeparam name="T">The type of the response data contained in the request.</typeparam>
    /// <param name="requestBuilder">A function that builds the request to be executed. Must not be <see langword="null"/>.</param>
    /// <param name="ifBeforePredicate">An optional predicate to evaluate before executing the request. If the predicate returns <see
    /// langword="false"/>, the request will not be executed.</param>
    /// <param name="ifAfterPredicate">An optional predicate to evaluate after executing the request. If the predicate returns <see langword="false"/>,
    /// the execution context will indicate a break condition.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <returns>An <see cref="ICommandExecutionContext"/> representing the execution context of the request.</returns>
    ICommandExecutionContext IfBreakRequest<T>(
       Func<IRequest<BaseListResponse<T>>> requestBuilder,
       Func<bool> ifBeforePredicate = null,
       Func<BaseListResponse<T>, bool> ifAfterPredicate = null,
       string blockName = null) where T : class =>
           WithRequest(requestBuilder, checkAfterExecution: ifAfterPredicate, checkBeforeExecution: ifBeforePredicate, blockName: blockName, continueOnCheckError: false);

    /// <summary>
    /// Executes a request conditionally based on the specified predicates.
    /// </summary>
    /// <remarks>This method allows conditional execution of a request by evaluating the provided predicates. 
    /// The <paramref name="ifBeforePredicate"/> is evaluated before the request is sent, and the  <paramref
    /// name="ifAfterPredicate"/> is evaluated after the request is executed. If either predicate  fails, the execution
    /// context will handle the error and continue processing.</remarks>
    /// <typeparam name="T">The type of the entity associated with the request. Must be a reference type.</typeparam>
    /// <param name="requestBuilder">A function that builds the request to be executed. This function must return an <see cref="IRequest{Response}"/>
    /// object.</param>
    /// <param name="ifBeforePredicate">An optional predicate that determines whether the request should be executed before it is sent.  If not
    /// provided, the request will always be executed.</param>
    /// <param name="ifAfterPredicate">An optional predicate that determines whether the request should be considered successful after execution, 
    /// based on the <see cref="Response"/> returned. If not provided, the request will always be considered successful.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <returns>An <see cref="ICommandExecutionContext"/> that represents the context of the executed command,  allowing further
    /// chaining or inspection of the execution.</returns>
    ICommandExecutionContext IfRequest<T>(
      Func<IRequest<Response>> requestBuilder,
      Func<bool> ifBeforePredicate = null,
      Func<Response, bool> ifAfterPredicate = null,
      string blockName = null) where T : class =>
          WithRequest(requestBuilder, checkAfterExecution: ifAfterPredicate, checkBeforeExecution: ifBeforePredicate, blockName: blockName, continueOnCheckError: true);

    /// <summary>
    /// Executes a request conditionally based on specified predicates. If false, break the whole execution.
    /// </summary>
    /// <remarks>This method allows conditional execution of a request based on predicates evaluated before
    /// and after the request execution. If either predicate is not satisfied, the execution context will reflect the
    /// failure state.</remarks>
    /// <typeparam name="T">The type of the context or data associated with the request. Must be a reference type.</typeparam>
    /// <param name="requestBuilder">A function that builds the request to be executed. This function must return an <see cref="IRequest{Response}"/>
    /// instance.</param>
    /// <param name="ifBeforePredicate">An optional predicate that is evaluated before the request is executed. If the predicate returns <see
    /// langword="false"/>, the request will not be executed. Defaults to <see langword="null"/>.</param>
    /// <param name="ifAfterPredicate">An optional predicate that is evaluated after the request is executed. If the predicate returns <see
    /// langword="false"/>, the execution context will indicate a failure. Defaults to <see langword="null"/>.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <returns>An <see cref="ICommandExecutionContext"/> representing the result of the conditional request execution.</returns>
    ICommandExecutionContext IfBreakRequest<T>(
      Func<IRequest<Response>> requestBuilder,
      Func<bool> ifBeforePredicate = null,
      Func<Response, bool> ifAfterPredicate = null,
      string blockName = null) where T : class =>
          WithRequest(requestBuilder, checkAfterExecution: ifAfterPredicate, checkBeforeExecution: ifBeforePredicate, blockName: blockName, continueOnCheckError: false);
}
