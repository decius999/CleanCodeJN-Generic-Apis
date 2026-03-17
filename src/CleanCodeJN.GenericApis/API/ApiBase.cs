using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Base controller class providing helper methods for dispatching MediatR requests and mapping HTTP responses.
/// Exceptions propagate to the registered <c>IExceptionHandler</c> pipeline.
/// Override <c>CleanCodeExceptionHandler</c> or register your own <c>IExceptionHandler</c> to customise error responses.
/// </summary>
public class ApiBase(IMediator commandBus, ICleanCodeMapper mapper) : ControllerBase
{
    /// <summary>
    /// Dispatches a list-response request and maps the resulting data to a paginated DTO response.
    /// </summary>
    public async Task<IResult> HandlePagination<TEntity, TDto>(IRequest<BaseListResponse<TEntity>> request)
    {
        var domainResult = await commandBus.Send(request);

        var dtos = mapper.Map<List<TDto>>(domainResult.Data);
        var response = await BaseListResponse<TDto>.Create(domainResult.ResultState, dtos, domainResult.Message, domainResult.Count);

        return domainResult.AsHttpResult(response);
    }

    /// <summary>
    /// Dispatches a list-response request and maps the resulting data to the specified DTO type.
    /// </summary>
    public async Task<IResult> Handle<TEntity, TDto>(IRequest<BaseListResponse<TEntity>> request, Func<BaseListResponse<TEntity>, TDto> map = null)
    {
        var domainResult = await commandBus.Send(request);

        return domainResult.AsHttpResult(map == null ? mapper.Map<TDto>(domainResult.Data) : map(domainResult));
    }

    /// <summary>
    /// Dispatches a single-entity request and maps the resulting data to the specified DTO type.
    /// </summary>
    public async Task<IResult> Handle<TEntity, TDto>(IRequest<BaseResponse<TEntity>> request, Func<BaseResponse<TEntity>, TDto> map = null)
        where TEntity : class
    {
        var domainResult = await commandBus.Send(request);

        return domainResult.AsHttpResult(map == null ? mapper.Map<TDto>(domainResult.Data) : map(domainResult));
    }

    /// <summary>
    /// Dispatches a void-style request that returns a plain <see cref="Response"/> and converts it to an HTTP result.
    /// </summary>
    public async Task<IResult> Handle<TEntity>(IRequest<Response> request)
        where TEntity : class
        => (await commandBus.Send(request)).AsHttpResult();
}
