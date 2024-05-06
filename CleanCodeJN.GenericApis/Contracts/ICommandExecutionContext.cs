﻿using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Context;
using MediatR;

namespace CleanCodeJN.GenericApis.Contracts;

public interface ICommandExecutionContext
{
    T Get<T>(string blockName) where T : class;
    List<T> GetList<T>(string blockName);
    Task<BaseResponse<T>> Execute<T>(CancellationToken cancellationToken) where T : class;
    Task<BaseListResponse<T>> ExecuteList<T>(CancellationToken cancellationToken);
    Task<Response> Execute(CancellationToken cancellationToken);
    CommandExecutionContext WithRequest<T>(Func<IRequest<BaseListResponse<T>>> requestBuilder, string blockName = null, Func<bool> checkBeforeExecution = null, Func<BaseListResponse<T>, bool> checkAfterExecution = null, bool? continueOnCheckError = false);
    CommandExecutionContext WithRequest<T>(Func<IRequest<BaseResponse<T>>> requestBuilder, string blockName = null, Func<bool> checkBeforeExecution = null, Func<BaseResponse<T>, bool> checkAfterExecution = null, bool? continueOnCheckError = false)
        where T : class;
    CommandExecutionContext WithRequests(Func<List<IRequest<Response>>> requestBuilder, string blockName = null, bool? continueOnCheckError = false);
    CommandExecutionContext WithRequest(Func<IRequest<Response>> requestBuilder, string blockName = null, Func<bool> checkBeforeExecution = null, Func<Response, bool> checkAfterExecution = null, bool? continueOnCheckError = false);
}