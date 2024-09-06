using System.Reflection;
using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Context;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using CleanCodeJN.Repository.EntityFramework.Extensions;
using FluentValidation;
using MediatR;

namespace CleanCodeJN.GenericApis.Extensions;

public static class ServiveCollectionExtensions
{
    /// <summary>
    /// Register Generic Apis DbContext, generic Repositories with Automatic mapping.
    /// </summary>
    /// <typeparam name="TDataContext">DbContext with inherits IDataContext</typeparam>
    /// <param name="services">Service Collection</param>
    /// <param name="mappingOverrides">Optional Automapper Mapping Action: Only for specific overrides. All standard ReverseMap() mappings will be created automatically</param>
    /// <param name="commandAssemblies">Optional: Additional Custom Assemblies to register custom Commands</param>
    public static void RegisterRepositoriesCommandsWithAutomaticMapping<TDataContext>(this IServiceCollection services, Action<IMapperConfigurationExpression> mappingOverrides = null, List<Assembly> commandAssemblies = null)
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

        services.AddTransient<ICommandExecutionContext, CommandExecutionContext>();

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies.ToArray());
            config.Lifetime = ServiceLifetime.Scoped;
        });

        services.AddValidatorsFromAssembly(Assembly.GetCallingAssembly());

        RegisterGenericCommands(services, assemblies);

        RegisterAutomapper(services, Scan(mappingOverrides, assemblies));

        services.RegisterDbContextAndRepositories<TDataContext>();
    }

    private static Action<IMapperConfigurationExpression> Scan(Action<IMapperConfigurationExpression> mapping, List<Assembly> assemblies)
    {
        var entities = GetTypesImplementingInterfaces(assemblies, typeof(IEntity)).ToDictionary(k => k.Name, v => v);
        var dtos = GetTypesImplementingInterfaces(assemblies, typeof(IDto)).ToDictionary(k => k.Name, v => v);
        List<Action<IMapperConfigurationExpression>> mappingConfigs = [];

        foreach (var entity in entities)
        {
            foreach (var dto in dtos)
            {
                if (dto.Key.StartsWith(entity.Key))
                {
                    mappingConfigs.Add(cfg => cfg.CreateMap(entity.Value, dto.Value).ReverseMap());
                }
            }
        }

        void combinedAction(IMapperConfigurationExpression cfg)
        {
            foreach (var action in mappingConfigs)
            {
                action(cfg);
            }

            mapping?.Invoke(cfg);
        }

        return combinedAction;
    }

    private static void RegisterAutomapper(this IServiceCollection services, Action<IMapperConfigurationExpression> mapping)
        => services.AddSingleton<IMapper>(new Mapper(new MapperConfiguration(mapping)));

    private static void RegisterGenericCommands(IServiceCollection services, List<Assembly> assemblies)
    {
        var entities = GetTypesImplementingInterfaces(assemblies, typeof(IEntity));
        var dtos = GetTypesImplementingInterfaces(assemblies, typeof(IDto));

        foreach (var entityType in entities)
        {
            var dtoPostType = dtos.FirstOrDefault(x => x.Name == entityType.Name + "Post" + "Dto");
            var dtoPutType = dtos.FirstOrDefault(x => x.Name == entityType.Name + "Put" + "Dto");
            var idType = entityType.GetProperties().First(x => x.Name == "Id").PropertyType;

            Register(services, entityType, idType);

            if (dtoPostType is not null)
            {
                var (handler, command) = PostTypes(entityType, dtoPostType, idType);
                services.AddScoped(handler, command);
            }

            if (dtoPutType is not null)
            {
                var (handler, command) = PutTypes(entityType, dtoPutType, idType);
                services.AddScoped(handler, command);
            }
        }
    }

    private static void Register(IServiceCollection services, Type entityType, Type idType)
    {
        var (handler, command) = GetTypes(entityType, idType);
        var (handlerById, commandById) = GetByIdTypes(entityType, idType);
        var getByIdsTypes = GetByIdsTypes(entityType, idType);
        var deleteTypes = DeleteTypes(entityType, idType);

        services.AddScoped(deleteTypes.handler, deleteTypes.command);
        services.AddScoped(handlerById, commandById);
        services.AddScoped(getByIdsTypes.handler, getByIdsTypes.command);
        services.AddScoped(handler, command);
    }

    private static IEnumerable<Type> GetTypesImplementingInterfaces(List<Assembly> assemblies, params Type[] interfaces) => assemblies.SelectMany(x => x.GetTypes())
                .Where(type => interfaces.All(i => i.IsAssignableFrom(type)))
                .ToList();

    private static (Type handler, Type command) MakeGenericTypeWith2Arguments(Type type, Type idType, Type requestType, Type commandType, Type responseType, Type handlerType)
    {
        var typeArgs = new List<Type> { type, idType }.ToArray();
        var requestGenericType = requestType.MakeGenericType(typeArgs);
        var commandGenericType = commandType.MakeGenericType(typeArgs);
        var responseGenericType = responseType.MakeGenericType([type]);
        var handlerGenericType = handlerType.MakeGenericType(requestGenericType, responseGenericType);

        return (handlerGenericType, commandGenericType);
    }

    private static (Type handler, Type command) GetTypes(Type type, Type idType)
    {
        var requestType = typeof(GetRequest<,>);
        var commandType = typeof(GetCommand<,>);
        var responseType = typeof(BaseListResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var (handler, command) = MakeGenericTypeWith2Arguments(type, idType, requestType, commandType, responseType, handlerType);

        return (handler, command);
    }

    private static (Type handler, Type command) DeleteTypes(Type type, Type idType)
    {
        var requestType = typeof(DeleteRequest<,>);
        var commandType = typeof(DeleteCommand<,>);
        var responseType = typeof(BaseResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var (handler, command) = MakeGenericTypeWith2Arguments(type, idType, requestType, commandType, responseType, handlerType);

        return (handler, command);
    }

    private static (Type handler, Type command) GetByIdTypes(Type type, Type idType)
    {
        var requestType = typeof(GetByIdRequest<,>);
        var commandType = typeof(GetByIdCommand<,>);
        var responseType = typeof(BaseResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var (handler, command) = MakeGenericTypeWith2Arguments(type, idType, requestType, commandType, responseType, handlerType);

        return (handler, command);
    }

    private static (Type handler, Type command) GetByIdsTypes(Type type, Type idType)
    {
        var requestType = typeof(GetByIdsRequest<,>);
        var commandType = typeof(GetByIdsCommand<,>);
        var responseType = typeof(BaseListResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var (handler, command) = MakeGenericTypeWith2Arguments(type, idType, requestType, commandType, responseType, handlerType);

        return (handler, command);
    }

    private static (Type handler, Type command) PostTypes(Type type, Type dtoType, Type idType)
    {
        var requestType = typeof(PostRequest<,>);
        var commandType = typeof(PostCommand<,,>);
        var responseType = typeof(BaseResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var typeArgs = new List<Type> { type }.ToArray();
        var requestGenericType = requestType.MakeGenericType(new[] { type, dtoType });
        var commandGenericType = commandType.MakeGenericType(new[] { type, dtoType, idType });
        var responseGenericType = responseType.MakeGenericType(typeArgs);
        var handlerGenericType = handlerType.MakeGenericType(requestGenericType, responseGenericType);

        return (handlerGenericType, commandGenericType);
    }

    private static (Type handler, Type command) PutTypes(Type type, Type dtoType, Type idType)
    {
        var requestType = typeof(PutRequest<,>);
        var commandType = typeof(PutCommand<,,>);
        var responseType = typeof(BaseResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var typeArgs = new List<Type> { type }.ToArray();
        var requestGenericType = requestType.MakeGenericType(new[] { type, dtoType });
        var commandGenericType = commandType.MakeGenericType(new[] { type, dtoType, idType });
        var responseGenericType = responseType.MakeGenericType(typeArgs);
        var handlerGenericType = handlerType.MakeGenericType(requestGenericType, responseGenericType);

        return (handlerGenericType, commandGenericType);
    }
}
