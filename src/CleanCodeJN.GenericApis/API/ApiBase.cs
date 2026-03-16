using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Base controller class providing helper methods for dispatching MediatR requests and mapping HTTP responses.
/// </summary>
public class ApiBase(IMediator commandBus, ICleanCodeMapper mapper) : ControllerBase
{
    /// <summary>
    /// Dispatches a list-response request and maps the resulting data to a paginated DTO response.
    /// </summary>
    /// <typeparam name="TEntity">The entity type returned by the handler.</typeparam>
    /// <typeparam name="TDto">The DTO type to map the response data to.</typeparam>
    /// <param name="request">The MediatR request producing a <see cref="BaseListResponse{TEntity}"/>.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response with pagination metadata.</returns>
    public async Task<IResult> HandlePagination<TEntity, TDto>(IRequest<BaseListResponse<TEntity>> request)
    {
        try
        {
            var domainResult = await commandBus.Send(request);

            var dtos = mapper.Map<List<TDto>>(domainResult.Data);
            var response = await BaseListResponse<TDto>.Create(domainResult.ResultState, dtos, domainResult.Message, domainResult.Count);

            return domainResult.AsHttpResult(response);
        }
        catch (Exception e)
        {
            return Results.Problem(title: e.Message, detail: e.StackTrace, statusCode: 400);
        }
    }

    /// <summary>
    /// Dispatches a list-response request and maps the resulting data to the specified DTO type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type returned by the handler.</typeparam>
    /// <typeparam name="TDto">The DTO type to map the response data to.</typeparam>
    /// <param name="request">The MediatR request producing a <see cref="BaseListResponse{TEntity}"/>.</param>
    /// <param name="map">An optional custom mapping function; if null, AutoMapper is used.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public async Task<IResult> Handle<TEntity, TDto>(IRequest<BaseListResponse<TEntity>> request, Func<BaseListResponse<TEntity>, TDto> map = null)
    {
        try
        {
            var domainResult = await commandBus.Send(request);

            return domainResult.AsHttpResult(map == null ? mapper.Map<TDto>(domainResult.Data) : map(domainResult));
        }
        catch (Exception e)
        {
            return Results.Problem(title: e.Message, detail: e.StackTrace, statusCode: 400);
        }
    }

    /// <summary>
    /// Dispatches a single-entity request and maps the resulting data to the specified DTO type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type returned by the handler.</typeparam>
    /// <typeparam name="TDto">The DTO type to map the response data to.</typeparam>
    /// <param name="request">The MediatR request producing a <see cref="BaseResponse{TEntity}"/>.</param>
    /// <param name="map">An optional custom mapping function; if null, AutoMapper is used.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public async Task<IResult> Handle<TEntity, TDto>(IRequest<BaseResponse<TEntity>> request, Func<BaseResponse<TEntity>, TDto> map = null)
        where TEntity : class
    {
        try
        {
            var domainResult = await commandBus.Send(request);

            return domainResult.AsHttpResult(map == null ? mapper.Map<TDto>(domainResult.Data) : map(domainResult));
        }
        catch (Exception e)
        {
            return Results.Problem(title: e.Message, detail: e.StackTrace, statusCode: 400);
        }
    }

    /// <summary>
    /// Dispatches a void-style request that returns a plain <see cref="Response"/> and converts it to an HTTP result.
    /// </summary>
    /// <typeparam name="TEntity">The entity type associated with the request.</typeparam>
    /// <param name="request">The MediatR request producing a <see cref="Response"/>.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public async Task<IResult> Handle<TEntity>(IRequest<Response> request)
        where TEntity : class
    {
        try
        {
            return (await commandBus.Send(request)).AsHttpResult();
        }
        catch (Exception e)
        {
            return Results.Problem(title: e.Message, detail: e.StackTrace, statusCode: 400);
        }
    }
}
