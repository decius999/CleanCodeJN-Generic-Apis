﻿using System.Linq.Expressions;
using System.Reflection;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Contracts;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.Extensions;

public static class MinimalAPIExtensions
{
    public static RouteHandlerBuilder MapGet<TEntity, TGetDto, TKey>(
        this WebApplication app,
        string route,
        List<string> tags,
        List<Expression<Func<TEntity, object>>> includes = null,
        Expression<Func<TEntity, bool>> where = null,
        Expression<Func<TEntity, TEntity>> select = null,
        bool asNoTracking = true,
        bool ignoreQueryFilters = false,
        bool asSplitQuery = true)
        where TEntity : class
        where TGetDto : class, IDto => app.MapGet(route, async ([FromServices] GetBase<TEntity, TGetDto> service) =>
        {
            service.Includes = includes;
            service.Where = where;
            service.Select = select;
            return await service.Get<TKey>(asNoTracking, ignoreQueryFilters, asSplitQuery);
        }).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetPaged<TEntity, TGetDto, TKey>(
        this WebApplication app,
        string route,
        List<string> tags,
        List<Expression<Func<TEntity, object>>> includes = null,
        Expression<Func<TEntity, bool>> where = null,
        Expression<Func<TEntity, TEntity>> select = null,
        bool asNoTracking = true,
        bool ignoreQueryFilters = false,
        bool asSplitQuery = true)
        where TEntity : class
        where TGetDto : class, IDto => app.MapGet(route + "/paged", async (int page, int pageSize, string direction, string sortBy, [FromServices] GetBase<TEntity, TGetDto> service) =>
        {
            service.Includes = includes;
            service.Where = where;
            service.Select = select;
            return await service.Get<TKey>(page, pageSize, direction, sortBy, asNoTracking, ignoreQueryFilters, asSplitQuery);
        }).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetFiltered<TEntity, TGetDto, TKey>(
        this WebApplication app,
        string route,
        List<string> tags,
        List<Expression<Func<TEntity, object>>> includes = null,
        Expression<Func<TEntity, bool>> where = null,
        Expression<Func<TEntity, TEntity>> select = null,
        bool asNoTracking = true,
        bool ignoreQueryFilters = false,
        bool asSplitQuery = true)
       where TEntity : class
       where TGetDto : class, IDto => app.MapGet(route + "/filtered", async (int page, int pageSize, string direction, string sortBy, string filter, [FromServices] GetBase<TEntity, TGetDto> service) =>
       {
           service.Includes = includes;
           service.Where = where;
           service.Select = select;
           return await service.Get<TKey>(page, pageSize, direction, sortBy, filter, asNoTracking, ignoreQueryFilters, asSplitQuery);
       }).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
      => app.MapGet(route, handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetRequest<TEntity, TDto>(this WebApplication app, string route, List<string> tags, Func<IRequest<BaseListResponse<TEntity>>> request) where TEntity : class, IEntity
        => app.MapGet(route, async ([FromServices] ApiBase api) => await api.Handle<TEntity, TDto>(request())).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetById<TEntity, TGetDto, TKey>(this WebApplication app, string route, List<string> tags, List<Expression<Func<TEntity, object>>> includes = null, Expression<Func<TEntity, bool>> where = null, bool asNoTracking = true, bool ignoreQueryFilters = false, bool asSplitQuery = true)
        where TEntity : class
        where TGetDto : class, IDto => app.MapGet(route + "/{id}", async (TKey id, [FromServices] GetByIdBase<TEntity, TGetDto> service) =>
        {
            service.Includes = includes;
            service.Where = where;
            return await service.Get(id, asNoTracking, ignoreQueryFilters, asSplitQuery);
        }).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetByIdRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
        => app.MapGet(route + "/{id}", handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetByIdRequest<TEntity, TDto, TKey>(this WebApplication app, string route, List<string> tags, Func<TKey, IRequest<BaseResponse<TEntity>>> request) where TEntity : class, IEntity
      => app.MapGet(route + "/{id}", async (TKey id, [FromServices] ApiBase api) => await api.Handle<TEntity, TDto>(request(id))).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPut<TEntity, TPutDto, TGetDto>(this WebApplication app, string route, List<string> tags)
        where TEntity : class
        where TGetDto : class, IDto
        where TPutDto : class, IDto => app.MapPut(route, async (TPutDto dto, [FromServices] PutBase<TEntity, TPutDto, TGetDto> service) =>
        await service.Put(dto)).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPutRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
       => app.MapPut(route, handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPutRequest<TEntity, TPutDto, TGetDto>(this WebApplication app, string route, List<string> tags, Func<TPutDto, IRequest<BaseResponse<TEntity>>> request) where TEntity : class, IEntity
      => app.MapPut(route, async (TPutDto dto, [FromServices] ApiBase api) =>
              await api.Handle<TEntity, TGetDto>(request(dto))).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPatch<TEntity, TGetDto, TKey>(this WebApplication app, string route, List<string> tags)
       where TEntity : class
       where TGetDto : class, IDto => app.MapPatch(route + "/{id}", async (TKey id, HttpContext httpContext, [FromServices] PatchBase<TEntity, TGetDto, TKey> service) =>
       await service.Patch(id, httpContext))
        .WithTags(tags.ToArray())
        .Accepts<JsonPatchDocument<TEntity>>("application/json-patch+json");

    public static RouteHandlerBuilder MapPatchRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
       => app.MapPatch(route + "/{id}", handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPatchRequest<TEntity, TGetDto, TKey>(this WebApplication app, string route, List<string> tags,
        Func<TKey, HttpContext, IRequest<BaseResponse<TEntity>>> request)
       where TEntity : class
       where TGetDto : class, IDto
    => app.MapPatch(route + "/{id}", async (TKey id, HttpContext httpContext, [FromServices] ApiBase api) =>
            await api.Handle<TEntity, TGetDto>(request(id, httpContext)))
                .WithTags(tags.ToArray())
                .Accepts<JsonPatchDocument<TEntity>>("application/json-patch+json");

    public static RouteHandlerBuilder MapPost<TEntity, TPostDto, TGetDto>(this WebApplication app, string route, List<string> tags)
        where TEntity : class
        where TGetDto : class, IDto
        where TPostDto : class, IDto => app.MapPost(route, async (TPostDto dto, [FromServices] PostBase<TEntity, TPostDto, TGetDto> service) => await service.Post(dto)).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPostRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
       => app.MapPost(route, handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPostRequest<TEntity, TPostDto, TGetDto>(this WebApplication app, string route, List<string> tags, Func<TPostDto, IRequest<BaseResponse<TEntity>>> request) where TEntity : class, IEntity
      => app.MapPost(route, async (TPostDto dto, [FromServices] ApiBase api) =>
              await api.Handle<TEntity, TGetDto>(request(dto))).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapDelete<TEntity, TGetDto, TKey>(this WebApplication app, string route, List<string> tags)
        where TEntity : class
        where TGetDto : class, IDto => app.MapDelete(route, async (TKey id, [FromServices] DeleteBase<TEntity, TGetDto> service) => await service.Delete(id)).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapDeleteRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
        => app.MapDelete(route + "/{id}", handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapDeleteRequest<TEntity, TDto, TKey>(this WebApplication app, string route, List<string> tags, Func<TKey, IRequest<BaseResponse<TEntity>>> request) where TEntity : class, IEntity<TKey>
        => app.MapDelete(route + "/{id}", async (TKey id, [FromServices] ApiBase api) =>
                await api.Handle<TEntity, TDto>(request(id))).WithTags(tags.ToArray());

    /// <summary>
    /// Use CleanCodeJN Generic Apis and Register all IApi Minimal API Instances
    /// </summary>
    /// <param name="app">The Web Application</param>
    /// <returns>Web Application</returns>
    public static WebApplication UseCleanCodeJNWithMinimalApis(this WebApplication app)
    {
        var interfaceType = typeof(IApi);
        var assembly = Assembly.GetCallingAssembly();

        var implementations = assembly.GetTypes().Where(
            t => interfaceType.IsAssignableFrom(t) &&
            t != interfaceType &&
            !t.IsGenericType);

        foreach (var implementation in implementations)
        {
            var api = (IApi)Activator.CreateInstance(implementation);
            foreach (var method in api.HttpMethods)
            {
                method(app);
            }
        }

        return app;
    }
}
