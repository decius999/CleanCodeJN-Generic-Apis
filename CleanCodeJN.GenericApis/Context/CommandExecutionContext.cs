using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Context;

public class CommandExecutionContext(IMediator commandBus) : ICommandExecutionContext
{
    private readonly List<(Delegate func, string blockName, Delegate checkBeforeExecution, Delegate checkAfterExecution, bool continueOnCheckError)> _requestBuilders = [];
    private readonly Dictionary<string, object> _responseCache = [];

    public ICommandExecutionContext WithRequest<T>(Func<IRequest<BaseListResponse<T>>> requestBuilder, string blockName = null,
       Func<bool> checkBeforeExecution = null, Func<BaseListResponse<T>, bool> checkAfterExecution = null, bool? continueOnCheckError = false)
    {
        _requestBuilders.Add((requestBuilder, blockName, checkBeforeExecution, checkAfterExecution, continueOnCheckError.Value));
        return this;
    }

    public ICommandExecutionContext WithRequest<T>(Func<IRequest<BaseResponse<T>>> requestBuilder, string blockName = null,
        Func<bool> checkBeforeExecution = null, Func<BaseResponse<T>, bool> checkAfterExecution = null, bool? continueOnCheckError = false)
          where T : class
    {
        _requestBuilders.Add((requestBuilder, blockName, checkBeforeExecution, checkAfterExecution, continueOnCheckError.Value));
        return this;
    }

    public ICommandExecutionContext WithRequest(Func<IRequest<Response>> requestBuilder, string blockName = null, Func<bool> checkBeforeExecution = null, Func<Response, bool> checkAfterExecution = null, bool? continueOnCheckError = false)
    {
        _requestBuilders.Add((requestBuilder, blockName, checkBeforeExecution, checkAfterExecution, continueOnCheckError.Value));
        return this;
    }

    public ICommandExecutionContext WithRequests(Func<List<IRequest<Response>>> requestBuilder, string blockName = null, bool? continueOnCheckError = false)
    {
        _requestBuilders.Add((requestBuilder, blockName, null, null, continueOnCheckError.Value));
        return this;
    }

    public async Task<BaseResponse<T>> Execute<T>(CancellationToken cancellationToken)
          where T : class
    {
        dynamic response = null;
        foreach (var builder in _requestBuilders)
        {
            response = await CreateRequestAndCallSend(builder, cancellationToken);

            if (response == null && !builder.continueOnCheckError)
            {
                return await BaseResponse<T>.Create(ResultEnum.FAILURE_BAD_REQUEST, message: $"Pre/Post condition fails in: {builder.blockName}", info: builder.blockName);
            }

            if (response is Response baseResponse && !baseResponse.Succeeded && !builder.continueOnCheckError)
            {
                var request = builder.func.DynamicInvoke();

                return await BaseResponse<T>.Create(baseResponse.ResultState, message: baseResponse.Message, info: builder.blockName);
            }

            if (response is Response interruptResponse && interruptResponse.Interrupt)
            {
                return await BaseResponse<T>.Create(interruptResponse.ResultState, message: interruptResponse.Message, info: builder.blockName, interrupt: interruptResponse.Interrupt);
            }

            AddToCache(response, builder);
        }

        var responseData = response?.Data as T;
        var responseInfo = response?.Info as string;

        return await BaseResponse<T>.Create(ResultEnum.SUCCESS, data: responseData, info: responseInfo);
    }

    public async Task<BaseListResponse<T>> ExecuteList<T>(CancellationToken cancellationToken)
    {
        dynamic response = null;
        foreach (var builder in _requestBuilders)
        {
            response = await CreateRequestAndCallSend(builder, cancellationToken);

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

    public async Task<Response> Execute(CancellationToken cancellationToken)
    {
        dynamic response = null;
        foreach (var builder in _requestBuilders)
        {
            response = await CreateRequestAndCallSend(builder, cancellationToken);

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

    public T Get<T>(string blockName)
        where T : class => _responseCache.TryGetValue(blockName, out var response) ? ((response as BaseResponse<T>)?.Data) : default;

    public List<T> GetList<T>(string blockName) => _responseCache.TryGetValue(blockName, out var response) ? ((response as BaseListResponse<T>)?.Data) : default;

    private void AddToCache(dynamic response, (Delegate func, string blockName, Delegate checkBeforeExecution, Delegate checkAfterExecution, bool continueOnCheckError) builder)
    {
        if (!string.IsNullOrWhiteSpace(builder.blockName) &&
            !_responseCache.TryGetValue(builder.blockName, out var _))
        {
            _responseCache.Add(builder.blockName, response);
        }
    }

    private async Task<dynamic> CreateRequestAndCallSend((Delegate func, string blockName, Delegate checkBeforeExecution, Delegate checkAfterExecution, bool continueOnCheckError) builder, CancellationToken cancellationToken)
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
