using System.Reflection;
using AutoMapper;
using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Context;
using CleanCodeJN.GenericApis.Contracts;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using CleanCodeJN.Repository.EntityFramework.Extensions;
using MediatR;

namespace CleanCodeJN.GenericApis.Extensions;

public static class ServiveCollectionExtensions
{
    /// <summary>
    /// Register Generic Apis DbContext, generic Repositories and automapper.
    /// </summary>
    /// <typeparam name="TDataContext">DbContext with inherits IDataContext</typeparam>
    /// <param name="services">Service Collection</param>
    /// <param name="mapping">Automapper Mapping Action</param>
    /// <param name="commandAssemblies">Additional Custom Assemblies to register custom Commands</param>
    public static void RegisterRepositoriesCommandsWithAutomapper<TDataContext>(this IServiceCollection services, Action<IMapperConfigurationExpression> mapping, List<Assembly> commandAssemblies = null)
        where TDataContext : class, IDataContext
    {
        commandAssemblies ??= [];
        List<Assembly> assemblies = [typeof(ApiBase).Assembly, Assembly.GetCallingAssembly(), .. commandAssemblies];

        services.AddScoped(typeof(GetBase<,>), typeof(Get<,>));
        services.AddScoped(typeof(GetByIdBase<,>), typeof(GetById<,>));
        services.AddScoped(typeof(PutBase<,,>), typeof(Put<,,>));
        services.AddScoped(typeof(PostBase<,,>), typeof(Post<,,>));
        services.AddScoped(typeof(DeleteBase<,>), typeof(Delete<,>));
        services.AddScoped(typeof(ApiBase));

        services.AddScoped<ICommandExecutionContext, CommandExecutionContext>();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies.ToArray());
            config.Lifetime = ServiceLifetime.Scoped;
        });

        RegisterGenericCommands(services, assemblies);

        var config = new MapperConfiguration(mapping);
        var mapper = new Mapper(config);

        services.AddSingleton<IMapper>(mapper);

        services.RegisterDbContextAndRepositories<TDataContext>();
    }

    private static void RegisterGenericCommands(IServiceCollection services, List<Assembly> assemblies)
    {
        var entities = GetTypesImplementingInterfaces(assemblies, typeof(IEntity<int>));
        var dtos = GetTypesImplementingInterfaces(assemblies, typeof(IDto));

        foreach (var entityType in entities)
        {
            var dtoPostType = dtos.FirstOrDefault(x => x.Name == entityType.Name + "Post" + "Dto");
            var dtoPutType = dtos.FirstOrDefault(x => x.Name == entityType.Name + "Put" + "Dto");

            var (handler, command) = GetTypes(entityType);
            var getByIdTypes = GetByIdTypes(entityType);
            var getByIdsTypes = GetByIdsTypes(entityType);
            var deleteTypes = DeleteTypes(entityType);

            services.AddScoped(handler, command);
            services.AddScoped(deleteTypes.handler, deleteTypes.command);
            services.AddScoped(getByIdTypes.handler, getByIdTypes.command);
            services.AddScoped(getByIdsTypes.handler, getByIdsTypes.command);

            if (dtoPostType is not null)
            {
                var postTypes = PostTypes(entityType, dtoPostType);
                services.AddScoped(postTypes.handler, postTypes.command);
            }

            if (dtoPutType is not null)
            {
                var putTypes = PutTypes(entityType, dtoPutType);
                services.AddScoped(putTypes.handler, putTypes.command);
            }
        }
    }

    private static IEnumerable<Type> GetTypesImplementingInterfaces(List<Assembly> assemblies, params Type[] interfaces) => assemblies.SelectMany(x => x.GetTypes())
                .Where(type => interfaces.All(i => i.IsAssignableFrom(type)))
                .ToList();

    private static (Type handler, Type command) MakeGenericType(Type type, Type requestType, Type commandType, Type responseType, Type handlerType)
    {
        var typeArgs = new List<Type> { type }.ToArray();
        var requestGenericType = requestType.MakeGenericType(typeArgs);
        var commandGenericType = commandType.MakeGenericType(typeArgs);
        var responseGenericType = responseType.MakeGenericType(typeArgs);
        var handlerGenericType = handlerType.MakeGenericType(requestGenericType, responseGenericType);

        return (handlerGenericType, commandGenericType);
    }

    private static (Type handler, Type command) GetTypes(Type type)
    {
        var requestType = typeof(GetRequest<>);
        var commandType = typeof(GetCommand<>);
        var responseType = typeof(BaseListResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var (handler, command) = MakeGenericType(type, requestType, commandType, responseType, handlerType);

        return (handler, command);
    }

    private static (Type handler, Type command) DeleteTypes(Type type)
    {
        var requestType = typeof(DeleteRequest<>);
        var commandType = typeof(DeleteCommand<>);
        var responseType = typeof(BaseResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var (handler, command) = MakeGenericType(type, requestType, commandType, responseType, handlerType);

        return (handler, command);
    }

    private static (Type handler, Type command) GetByIdTypes(Type type)
    {
        var requestType = typeof(GetByIdRequest<>);
        var commandType = typeof(GetByIdCommand<>);
        var responseType = typeof(BaseResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var (handler, command) = MakeGenericType(type, requestType, commandType, responseType, handlerType);

        return (handler, command);
    }

    private static (Type handler, Type command) GetByIdsTypes(Type type)
    {
        var requestType = typeof(GetByIdsRequest<>);
        var commandType = typeof(GetByIdsCommand<>);
        var responseType = typeof(BaseListResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var (handler, command) = MakeGenericType(type, requestType, commandType, responseType, handlerType);

        return (handler, command);
    }

    private static (Type handler, Type command) PostTypes(Type type, Type dtoType)
    {
        var requestType = typeof(PostRequest<,>);
        var commandType = typeof(PostCommand<,>);
        var responseType = typeof(BaseResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var typeArgs = new List<Type> { type }.ToArray();
        var requestGenericType = requestType.MakeGenericType(new[] { type, dtoType });
        var commandGenericType = commandType.MakeGenericType(new[] { type, dtoType });
        var responseGenericType = responseType.MakeGenericType(typeArgs);
        var handlerGenericType = handlerType.MakeGenericType(requestGenericType, responseGenericType);

        return (handlerGenericType, commandGenericType);
    }

    private static (Type handler, Type command) PutTypes(Type type, Type dtoType)
    {
        var requestType = typeof(PutRequest<,>);
        var commandType = typeof(PutCommand<,>);
        var responseType = typeof(BaseResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var typeArgs = new List<Type> { type }.ToArray();
        var requestGenericType = requestType.MakeGenericType(new[] { type, dtoType });
        var commandGenericType = commandType.MakeGenericType(new[] { type, dtoType });
        var responseGenericType = responseType.MakeGenericType(typeArgs);
        var handlerGenericType = handlerType.MakeGenericType(requestGenericType, responseGenericType);

        return (handlerGenericType, commandGenericType);
    }
}
