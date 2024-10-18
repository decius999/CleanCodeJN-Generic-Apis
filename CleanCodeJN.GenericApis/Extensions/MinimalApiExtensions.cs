using System.Linq.Expressions;
using System.Reflection;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.Extensions;

public static class MinimalAPIExtensions
{
    public static RouteHandlerBuilder MapGet<TEntity, TGetDto, TKey>(this WebApplication app, string route, List<string> tags, List<Expression<Func<TEntity, object>>> includes = null, Expression<Func<TEntity, bool>> where = null, bool asNoTracking = true, bool ignoreQueryFilters = false, bool asSplitQuery = true)
        where TEntity : class
        where TGetDto : class, IDto => app.MapGet(route, async ([FromServices] GetBase<TEntity, TGetDto> service) =>
        {
            service.Includes = includes;
            service.Where = where;
            return await service.Get<TKey>(asNoTracking, ignoreQueryFilters, asSplitQuery);
        }).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetPaged<TEntity, TGetDto, TKey>(this WebApplication app, string route, List<string> tags, List<Expression<Func<TEntity, object>>> includes = null, Expression<Func<TEntity, bool>> where = null, bool asNoTracking = true, bool ignoreQueryFilters = false, bool asSplitQuery = true)
        where TEntity : class
        where TGetDto : class, IDto => app.MapGet(route + "/paged", async (int page, int pageSize, string direction, string sortBy, [FromServices] GetBase<TEntity, TGetDto> service) =>
        {
            service.Includes = includes;
            service.Where = where;
            return await service.Get<TKey>(page, pageSize, direction, sortBy, asNoTracking, ignoreQueryFilters, asSplitQuery);
        }).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetFiltered<TEntity, TGetDto, TKey>(this WebApplication app, string route, List<string> tags, List<Expression<Func<TEntity, object>>> includes = null, Expression<Func<TEntity, bool>> where = null, bool asNoTracking = true, bool ignoreQueryFilters = false, bool asSplitQuery = true)
       where TEntity : class
       where TGetDto : class, IDto => app.MapGet(route + "/filtered", async (int page, int pageSize, string direction, string sortBy, string filter, [FromServices] GetBase<TEntity, TGetDto> service) =>
       {
           service.Includes = includes;
           service.Where = where;
           return await service.Get<TKey>(page, pageSize, direction, sortBy, filter, asNoTracking, ignoreQueryFilters, asSplitQuery);
       }).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
      => app.MapGet(route, handler).WithTags(tags.ToArray());

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

    public static RouteHandlerBuilder MapPut<TEntity, TPutDto, TGetDto>(this WebApplication app, string route, List<string> tags)
        where TEntity : class
        where TGetDto : class, IDto
        where TPutDto : class, IDto => app.MapPut(route, async (TPutDto dto, [FromServices] PutBase<TEntity, TPutDto, TGetDto> service) => await service.Put(dto)).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPutRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
        => app.MapPut(route, handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPost<TEntity, TPostDto, TGetDto>(this WebApplication app, string route, List<string> tags)
        where TEntity : class
        where TGetDto : class, IDto
        where TPostDto : class, IDto => app.MapPost(route, async (TPostDto dto, [FromServices] PostBase<TEntity, TPostDto, TGetDto> service) => await service.Post(dto)).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPostRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
       => app.MapPost(route, handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapDelete<TEntity, TGetDto, TKey>(this WebApplication app, string route, List<string> tags)
        where TEntity : class
        where TGetDto : class, IDto => app.MapDelete(route, async (TKey id, [FromServices] DeleteBase<TEntity, TGetDto> service) => await service.Delete(id)).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapDeleteRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
        => app.MapDelete(route + "/{id}", handler).WithTags(tags.ToArray());

    public static WebApplication RegisterApis(this WebApplication app)
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
