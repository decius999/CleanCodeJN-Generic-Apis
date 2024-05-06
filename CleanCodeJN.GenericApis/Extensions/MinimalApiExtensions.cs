using System.Reflection;
using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.Extensions;

public static class MinimalAPIExtensions
{
    public static RouteHandlerBuilder MapGet<TEntity, TGetDto>(this WebApplication app, string route, List<string> tags)
        where TEntity : class
        where TGetDto : class, IDto => app.MapGet(route, async ([FromServices] GetBase<TEntity, TGetDto> get) => await get.Get()).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
      => app.MapGet(route, handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetById<TEntity, TGetDto>(this WebApplication app, string route, List<string> tags)
        where TEntity : class
        where TGetDto : class, IDto => app.MapGet(route + "/{id:int}", async (int id, [FromServices] GetByIdBase<TEntity, TGetDto> getById) => await getById.Get(id)).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapGetByIdRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
        => app.MapGet(route + "/{id:int}", handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPut<TEntity, TPutDto, TGetDto>(this WebApplication app, string route, List<string> tags)
        where TEntity : class
        where TGetDto : class, IDto
        where TPutDto : class, IDto => app.MapPut(route, async (TPutDto dto, [FromServices] PutBase<TEntity, TPutDto, TGetDto> put) => await put.Put(dto)).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPutRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
        => app.MapPut(route, handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPost<TEntity, TPostDto, TGetDto>(this WebApplication app, string route, List<string> tags)
        where TEntity : class
        where TGetDto : class, IDto
        where TPostDto : class, IDto => app.MapPost(route, async (TPostDto dto, [FromServices] PostBase<TEntity, TPostDto, TGetDto> put) => await put.Post(dto)).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapPostRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
       => app.MapPost(route, handler).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapDelete<TEntity, TGetDto>(this WebApplication app, string route, List<string> tags)
        where TEntity : class
        where TGetDto : class, IDto => app.MapDelete(route, async (int id, [FromServices] DeleteBase<TEntity, TGetDto> delete) => await delete.Delete(id)).WithTags(tags.ToArray());

    public static RouteHandlerBuilder MapDeleteRequest(this WebApplication app, string route, List<string> tags, Delegate handler)
        => app.MapDelete(route, handler).WithTags(tags.ToArray());

    public static WebApplication RegisterApis(this WebApplication app)
    {
        var interfaceType = typeof(IApi);
        var assembly = Assembly.GetCallingAssembly();

        var implementations = assembly.GetTypes().Where(t => interfaceType.IsAssignableFrom(t) && t != interfaceType);

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
