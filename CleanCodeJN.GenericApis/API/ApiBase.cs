using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;
public class ApiBase : ControllerBase
{
    public IMediator CommandBus { get; }

    public IMapper Mapper { get; }

    public ApiBase(IMediator commandBus, IMapper mapper)
    {
        CommandBus = commandBus;
        Mapper = mapper;
    }

    public async Task<IResult> HandlePagination<TEntity, TDto>(IRequest<BaseListResponse<TEntity>> request)
    {
        try
        {
            var domainResult = await CommandBus.Send(request);

            var dtos = Mapper.Map<List<TDto>>(domainResult.Data);
            var response = await BaseListResponse<TDto>.Create(domainResult.ResultState, dtos, domainResult.Message, domainResult.Count);

            return domainResult.AsHttpResult(response);
        }
        catch (Exception e)
        {
            return Results.Problem(title: e.Message, detail: e.StackTrace, statusCode: 500);
        }
    }

    public async Task<IResult> Handle<TEntity, TDto>(IRequest<BaseListResponse<TEntity>> request, Func<BaseListResponse<TEntity>, TDto> map = null)
    {
        try
        {
            var domainResult = await CommandBus.Send(request);

            return domainResult.AsHttpResult(map == null ? Mapper.Map<TDto>(domainResult.Data) : map(domainResult));
        }
        catch (Exception e)
        {
            return Results.Problem(title: e.Message, detail: e.StackTrace, statusCode: 500);
        }
    }

    public async Task<IResult> Handle<TEntity, TDto>(IRequest<BaseResponse<TEntity>> request, Func<BaseResponse<TEntity>, TDto> map = null)
        where TEntity : class
    {
        try
        {
            var domainResult = await CommandBus.Send(request);

            return domainResult.AsHttpResult(map == null ? Mapper.Map<TDto>(domainResult.Data) : map(domainResult));
        }
        catch (Exception e)
        {
            return Results.Problem(title: e.Message, detail: e.StackTrace, statusCode: 500);
        }
    }

    public async Task<IResult> Handle<TEntity>(IRequest<Response> request)
        where TEntity : class
    {
        try
        {
            var domainResult = await CommandBus.Send(request);

            return domainResult.AsHttpResult();
        }
        catch (Exception e)
        {
            return Results.Problem(title: e.Message, detail: e.StackTrace, statusCode: 500);
        }
    }
}

