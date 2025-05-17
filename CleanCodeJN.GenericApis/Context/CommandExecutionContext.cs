using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Context;

/// <summary>
/// The Integration Operation Segregation Principle (IOSP) Execution Context. Used to seperate the execution of integration and operation request handler.
/// </summary>
/// <param name="commandBus">The mediatr service.</param>
public class CommandExecutionContext(IMediator commandBus) : ICommandExecutionContext
{
    private readonly List<(Delegate func, string blockName, Delegate checkBeforeExecution, Delegate checkAfterExecution, bool continueOnCheckError, Guid parallelId)> _requestBuilders = [];
    private readonly Dictionary<string, object> _responseCache = [];

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
    public ICommandExecutionContext WithRequest<T>(Func<IRequest<BaseListResponse<T>>> requestBuilder, string blockName = null,
       Func<bool> checkBeforeExecution = null, Func<BaseListResponse<T>, bool> checkAfterExecution = null, bool? continueOnCheckError = false)
    {
        _requestBuilders.Add((requestBuilder, blockName, checkBeforeExecution, checkAfterExecution, continueOnCheckError.Value, Guid.Empty));
        return this;
    }

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
    public ICommandExecutionContext WithRequest<T>(Func<IRequest<BaseResponse<T>>> requestBuilder, string blockName = null,
        Func<bool> checkBeforeExecution = null, Func<BaseResponse<T>, bool> checkAfterExecution = null, bool? continueOnCheckError = false)
          where T : class
    {
        _requestBuilders.Add((requestBuilder, blockName, checkBeforeExecution, checkAfterExecution, continueOnCheckError.Value, Guid.Empty));
        return this;
    }

    /// <summary>
    /// Add a request to the execution context with Response.
    /// </summary>
    /// <param name="requestBuilder">Lamda for constructing the IRequest of T type.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <param name="checkBeforeExecution">Lamda to check if this block should be executed.</param>
    /// <param name="checkAfterExecution">Lamda to check if after the execution the next block should be executed.</param>
    /// <param name="continueOnCheckError">True: Continue executing. False: Stop executing of blocks on errors.</param>
    /// <returns>ICommandExecutionContext for pipelining more blocks.</returns>
    public ICommandExecutionContext WithRequest(Func<IRequest<Response>> requestBuilder, string blockName = null, Func<bool> checkBeforeExecution = null, Func<Response, bool> checkAfterExecution = null, bool? continueOnCheckError = false)
    {
        _requestBuilders.Add((requestBuilder, blockName, checkBeforeExecution, checkAfterExecution, continueOnCheckError.Value, Guid.Empty));
        return this;
    }

    /// <summary>
    /// Add a request to the execution context with List of IRequest of Response.
    /// </summary>
    /// <param name="requestBuilder">Lamda for constructing the IRequest of T type.</param>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <param name="continueOnCheckError">True: Continue executing. False: Stop executing of blocks on errors.</param>
    /// <returns>ICommandExecutionContext for pipelining more blocks.</returns>
    public ICommandExecutionContext WithRequests(Func<List<IRequest<Response>>> requestBuilder, string blockName = null, bool? continueOnCheckError = false)
    {
        _requestBuilders.Add((requestBuilder, blockName, null, null, continueOnCheckError.Value, Guid.Empty));
        return this;
    }

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
    public ICommandExecutionContext WithParallelWhenAllRequests<T>(List<Func<IRequest<BaseResponse<T>>>> requestBuilder, string blockName = null, Func<bool> checkBeforeExecution = null, Func<BaseResponse<T>, bool> checkAfterExecution = null, bool? continueOnCheckError = false) where T : class
    {
        var guid = Guid.CreateVersion7();

        foreach (var request in requestBuilder)
        {
            _requestBuilders.Add((request, blockName, checkBeforeExecution, checkAfterExecution, continueOnCheckError.Value, guid));
        }

        return this;
    }

    /// <summary>
    /// Execute the commands in the context with BaseResponse.
    /// </summary>
    /// <typeparam name="T">The IRequest of T type.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>BaseResponse of T.</returns>
    public async Task<BaseResponse<T>> Execute<T>(CancellationToken cancellationToken)
          where T : class
    {
        dynamic response = null;
        var parallelIds = new HashSet<Guid>();

        foreach (var builder in _requestBuilders)
        {
            if (builder.parallelId != Guid.Empty && !parallelIds.Contains(builder.parallelId))
            {
                var tasks = new List<Task<dynamic>>();
                parallelIds.Add(builder.parallelId);

                foreach (var request in _requestBuilders.Where(x => x.parallelId == builder.parallelId))
                {
                    tasks.Add(CreateRequestAndCallSend(request, cancellationToken));
                }

                var results = await Task.WhenAll(tasks);

                var errors = string.Join(" - ",
                    results
                    .Where(response => response is Response parallelResponse && !parallelResponse.Succeeded)
                    .Select(x => x.Message)
                    .ToList());

                response = results.All(response => response is Response parallelResponse && parallelResponse.Succeeded)
                    ? await BaseListResponse<dynamic>.Create(true, data: results.ToList(), info: builder.blockName)
                    : (dynamic)await BaseListResponse<dynamic>.Create(
                        ResultEnum.FAILURE_BAD_REQUEST,
                        message: $"'Pre/Post condition' fails in: {builder.blockName}: {errors}",
                        info: builder.blockName);
            }
            else if (parallelIds.Contains(builder.parallelId))
            {
                continue;
            }
            else
            {
                response = await CreateRequestAndCallSend(builder, cancellationToken);
            }

            if (response == null && !builder.continueOnCheckError)
            {
                return await BaseResponse<T>.Create(
                    ResultEnum.FAILURE_BAD_REQUEST,
                    message: $"Pre/Post condition fails in: {builder.blockName}",
                    info: builder.blockName);
            }

            if (response is Response baseResponse && !baseResponse.Succeeded && !builder.continueOnCheckError)
            {
                return await BaseResponse<T>.Create(
                    baseResponse.ResultState,
                    message: baseResponse.Message,
                    info: builder.blockName);
            }

            if (response is Response interruptResponse && interruptResponse.Interrupt)
            {
                return await BaseResponse<T>.Create(
                    interruptResponse.ResultState,
                    message: interruptResponse.Message,
                    info: builder.blockName,
                    interrupt: interruptResponse.Interrupt);
            }

            AddToCache(response, builder);
        }

        var responseData = response?.Data as T;
        var responseInfo = response?.Info as string;

        return await BaseResponse<T>.Create(ResultEnum.SUCCESS, data: responseData, info: responseInfo);
    }

    /// <summary>
    /// Execute the commands in the context with BaseListResponse.
    /// </summary>
    /// <typeparam name="T">The IRequest of T type.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>BaseListResponse of T.</returns>
    public async Task<BaseListResponse<T>> ExecuteList<T>(CancellationToken cancellationToken)
    {
        dynamic response = null;
        var parallelIds = new HashSet<Guid>();

        foreach (var builder in _requestBuilders)
        {
            if (builder.parallelId != Guid.Empty && !parallelIds.Contains(builder.parallelId))
            {
                var tasks = new List<Task<dynamic>>();
                parallelIds.Add(builder.parallelId);

                foreach (var request in _requestBuilders.Where(x => x.parallelId == builder.parallelId))
                {
                    tasks.Add(CreateRequestAndCallSend(request, cancellationToken));
                }

                var results = await Task.WhenAll(tasks);

                var errors = string.Join(" - ",
                   results
                   .Where(response => response is Response parallelResponse && !parallelResponse.Succeeded)
                   .Select(x => x.Message)
                   .ToList());

                response = results.All(response => response is Response parallelResponse && parallelResponse.Succeeded)
                    ? await BaseListResponse<T>.Create(true, info: builder.blockName)
                    : (dynamic)await BaseListResponse<T>.Create(
                        ResultEnum.FAILURE_BAD_REQUEST,
                        message: $"Pre/Post condition fails in: {builder.blockName}: {errors}",
                        info: builder.blockName);
            }
            else if (parallelIds.Contains(builder.parallelId))
            {
                continue;
            }
            else
            {
                response = await CreateRequestAndCallSend(builder, cancellationToken);
            }

            if (response == null && !builder.continueOnCheckError)
            {
                return await BaseListResponse<T>.Create(ResultEnum.FAILURE_BAD_REQUEST, message: $"Pre/Post condition fails in: {builder.blockName}", info: builder.blockName);
            }

            if (!builder.continueOnCheckError && response is Response baseResponse && !baseResponse.Succeeded)
            {
                return await BaseListResponse<T>.Create(baseResponse.ResultState, message: baseResponse.Message, info: builder.blockName);
            }

            if (response is Response interruptResponse && interruptResponse.Interrupt)
            {
                return await BaseListResponse<T>.Create(interruptResponse.ResultState, message: interruptResponse.Message, info: builder.blockName, interrupt: interruptResponse.Interrupt);
            }

            AddToCache(response, builder);
        }

        return await BaseListResponse<T>.Create(ResultEnum.SUCCESS, data: response?.Data, message: response?.Message, count: response?.Count);
    }

    /// <summary>
    /// Execute the commands in the context with Response.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Response type for executing asynchronous events without returning data.</returns>
    public async Task<Response> Execute(CancellationToken cancellationToken)
    {
        dynamic response = null;
        var parallelIds = new HashSet<Guid>();

        foreach (var builder in _requestBuilders)
        {
            if (builder.parallelId != Guid.Empty && !parallelIds.Contains(builder.parallelId))
            {
                var tasks = new List<Task<dynamic>>();
                parallelIds.Add(builder.parallelId);

                foreach (var request in _requestBuilders.Where(x => x.parallelId == builder.parallelId))
                {
                    tasks.Add(CreateRequestAndCallSend(request, cancellationToken));
                }

                var results = await Task.WhenAll(tasks);

                var errors = string.Join(" - ",
                   results
                   .Where(response => response is Response parallelResponse && !parallelResponse.Succeeded)
                   .Select(x => x.Message)
                   .ToList());

                response = results.All(response => response is Response parallelResponse && parallelResponse.Succeeded)
                    ? new Response(ResultEnum.SUCCESS, info: builder.blockName)
                    : (dynamic)new Response(
                        ResultEnum.FAILURE_BAD_REQUEST,
                        message: $"Pre/Post condition fails in: {builder.blockName}: {errors}",
                        info: builder.blockName);
            }
            else if (parallelIds.Contains(builder.parallelId))
            {
                continue;
            }
            else
            {
                response = await CreateRequestAndCallSend(builder, cancellationToken);
            }

            if (response == null && !builder.continueOnCheckError)
            {
                return new Response(ResultEnum.FAILURE_BAD_REQUEST, message: $"Pre/Post condition fails in: {builder.blockName}", info: builder.blockName);
            }

            if (!builder.continueOnCheckError && response is Response baseResponse && !baseResponse.Succeeded && !baseResponse.Interrupt)
            {
                return new Response(baseResponse.ResultState, message: baseResponse.Message, interrupt: baseResponse.Interrupt, info: builder.blockName);
            }

            if (response is Response interruptResponse && interruptResponse.Interrupt)
            {
                return new Response(interruptResponse.ResultState, message: interruptResponse.Message, info: builder.blockName, interrupt: interruptResponse.Interrupt);
            }

            AddToCache(response, builder);
        }

        return new Response(ResultEnum.SUCCESS, message: response?.Message, count: response?.Count);
    }

    /// <summary>
    /// Get the response from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the object in the cache.</typeparam>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <returns>Type T.</returns>
    public T Get<T>(string blockName) where T : class =>
        _responseCache.TryGetValue(blockName, out var response) ? ((response as BaseResponse<T>)?.Data) : default;

    /// <summary>
    /// Get the list from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the object in the cache.</typeparam>
    /// <param name="blockName">The name of this specific block, which can be referenced.</param>
    /// <returns>List of type T.</returns>
    public List<T> GetList<T>(string blockName) =>
        _responseCache.TryGetValue(blockName, out var response) ? ((response as BaseListResponse<T>)?.Data) : default;

    private void AddToCache(dynamic response, (Delegate func, string blockName, Delegate checkBeforeExecution, Delegate checkAfterExecution, bool continueOnCheckError, Guid parallelId) builder)
    {
        if (!string.IsNullOrWhiteSpace(builder.blockName) &&
            !_responseCache.TryGetValue(builder.blockName, out var _))
        {
            _responseCache.Add(builder.blockName, response);
        }
    }

    private async Task<dynamic> CreateRequestAndCallSend((Delegate func, string blockName, Delegate checkBeforeExecution, Delegate checkAfterExecution, bool continueOnCheckError, Guid parallelId) builder, CancellationToken cancellationToken)
    {
        if (builder.checkBeforeExecution != null && !(bool)builder.checkBeforeExecution.DynamicInvoke())
        {
            return null;
        }

        object response;
        var request = builder.func.DynamicInvoke();

        if (request is List<IRequest<Response>>)
        {
            foreach (var requestItem in request as List<IRequest<Response>>)
            {
                var responseItem = await commandBus.Send(requestItem, cancellationToken);

                if (!((dynamic)responseItem).Success)
                {
                    return new Response(ResultEnum.FAILURE_BAD_REQUEST);
                }
            }

            return new Response(ResultEnum.SUCCESS);
        }
        else
        {
            response = await commandBus.Send(request, cancellationToken);
            return builder.checkAfterExecution != null && !(bool)builder.checkAfterExecution.DynamicInvoke(response) ? null : (dynamic)response;
        }
    }
}
