using System.Reflection;
using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Behaviors;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Context;
using CleanCodeJN.GenericApis.Services;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using CleanCodeJN.Repository.EntityFramework.Extensions;
using FluentValidation;
using HotChocolate.Execution.Configuration;
using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Service Collection Extensions.
/// </summary>
public static class ServiveCollectionExtensions
{
    /// <summary>
    /// Add Clean Code JN package with options.
    /// </summary>
    /// <typeparam name="TDataContext">DbContext with inherits IDataContext.</typeparam>
    /// <param name="services">Service Collection.</param>
    /// <param name="optionAction">The option Action to configure the package.</param>
    public static void AddCleanCodeJN<TDataContext>(this IServiceCollection services, Action<CleanCodeOptions> optionAction)
        where TDataContext : class, IDataContext
    {
        var options = new CleanCodeOptions();
        optionAction.Invoke(options);

        List<Assembly> assemblies = [typeof(ApiBase).Assembly, Assembly.GetCallingAssembly(), .. options.ApplicationAssemblies];

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
            };
        });

        if (options.UseDistributedMemoryCache)
        {
            services.AddDistributedMemoryCache();
        }

        if (options.GraphQLOptions is not null)
        {
            var schema = services
                .AddGraphQLServer()
                .AddQueryType(d => d.Name("Query"))
                .AddProjections()
                .AddFiltering()
                .AddSorting();

            if (options.GraphQLOptions.Create || options.GraphQLOptions.Update || options.GraphQLOptions.Delete)
            {
                schema.AddMutationType(d => d.Name("Mutation"));
            }

            if (options.GraphQLOptions.EnableIntrospection)
            {
                schema
                    .ModifyRequestOptions(o => o.IncludeExceptionDetails = true)
                    .AddErrorFilter<GraphQLStartupErrorFilter>()
                    .DisableIntrospection(false);
            }

            if (options.GraphQLOptions.AddAuthorizationWithPolicyName is not null)
            {
                schema.AddAuthorization();
            }

            schema.AddGraphQLGet(assemblies, options.GraphQLOptions);
            schema.AddGraphQLCreate(assemblies, options.GraphQLOptions);
            schema.AddGraphQLUpdate(assemblies, options.GraphQLOptions);
            schema.AddGraphQLDelete(assemblies, options.GraphQLOptions);
        }

        if (options.AiProxyOptions is not null)
        {
            services.AddHttpClient("AiProxy")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
            services.AddScoped(sp =>
                  new AiProxyService(
                      Options.Create(options.AiProxyOptions),
                      sp.GetRequiredService<IHttpClientFactory>(),
                      sp.GetRequiredService<ILogger<AiProxyService>>()
                  ));

            services.AddCors(options =>
            {
                options.AddPolicy("BlazorChat", policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
        }

        services
            .RegisterMinimalApiBaseClasses()
                .RegisterCommandExecutionContext()
                .RegisterMediatr(assemblies, options)
                .RegisterValidatorsFromAssembly(options.ValidatorAssembly)
                .RegisterGenericCommands(assemblies)
                .RegisterAutomapper(assemblies, Scan(options.MappingOverrides, assemblies))
                .RegisterDbContextAndRepositories<TDataContext>();
    }

    /// <summary>
    /// Register validators from assembly for Fluent Validation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="validatorAssembly">The Assembly where your Abstract Validators are located.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection RegisterValidatorsFromAssembly(this IServiceCollection services, Assembly validatorAssembly) =>
        services.AddValidatorsFromAssembly(validatorAssembly ?? Assembly.GetCallingAssembly());

    /// <summary>
    /// Registers MediatR with the configured behaviors and service assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies containing MediatR request and handler types.</param>
    /// <param name="options">The CleanCode options specifying behaviors to register.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection RegisterMediatr(
        this IServiceCollection services,
        List<Assembly> assemblies,
        CleanCodeOptions options) => services.AddMediatR(config =>
    {
        config.RegisterServicesFromAssemblies(assemblies.ToArray());
        config.AddOpenBehavior(typeof(CachingBehavior<,>));

        if (options.AddDefaultLoggingBehavior)
        {
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        }

        foreach (var type in options.OpenBehaviors)
        {
            config.AddOpenBehavior(type);
        }

        foreach (var type in options.ClosedBehaviors)
        {
            config.AddBehavior(type);
        }

        config.Lifetime = ServiceLifetime.Scoped;
    });

    /// <summary>
    /// Register Command Execution Context.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection RegisterCommandExecutionContext(this IServiceCollection services) => services.AddTransient<ICommandExecutionContext, CommandExecutionContext>();

    /// <summary>
    /// Register Minimal API Base Classes.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection RegisterMinimalApiBaseClasses(this IServiceCollection services)
    {
        services.AddScoped(typeof(GetBase<,>), typeof(Get<,>));
        services.AddScoped(typeof(GetByIdBase<,>), typeof(GetById<,>));
        services.AddScoped(typeof(PutBase<,,>), typeof(Put<,,>));
        services.AddScoped(typeof(PatchBase<,,>), typeof(Patch<,,>));
        services.AddScoped(typeof(PostBase<,,>), typeof(Post<,,>));
        services.AddScoped(typeof(DeleteBase<,>), typeof(Delete<,>));
        services.AddScoped(typeof(ApiBase));

        return services;
    }

    /// <summary>
    /// Register Automapper.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The Assemblies where your Entities, DTOs and Commands are located.</param>
    /// <param name="mapping">Optional: The Automapper Mapping Profile.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection RegisterAutomapper(this IServiceCollection services, List<Assembly> assemblies, Action<IMapperConfigurationExpression> mapping = null)
        => services.AddSingleton<IMapper>(
            mapping != null ?
            new Mapper(new MapperConfiguration(mapping)) :
            new Mapper(new MapperConfiguration(Scan(mapping, assemblies))));

    /// <summary>
    /// Configures the application to use GraphQL with the specified default route.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
    /// <param name="defaultRoute">The default route for the GraphQL endpoint. The default value is <c>"/graphql"</c>.</param>
    /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
    public static WebApplication UseCleanCodeJNWithGraphQL(this WebApplication app, string defaultRoute = "/graphql")
    {
        app.MapGraphQL(defaultRoute);
        return app;
    }

    /// <summary>
    /// Register Generic Commands.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The Assemblies where your Entities, DTOs and Commands are located.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection RegisterGenericCommands(this IServiceCollection services, List<Assembly> assemblies)
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

        return services;
    }

    private static void AddGraphQLGet(this IRequestExecutorBuilder schema, List<Assembly> assemblies, GraphQLOptions options)
    {
        if (options.Get)
        {
            var entities = GetTypesImplementingInterfaces(assemblies, typeof(IEntity)).ToList();
            var dtos = GetTypesImplementingInterfaces(assemblies, typeof(IDto)).ToList();

            var pairs = entities
                .Select(e => (
                    Entity: e,
                    Dto: dtos.FirstOrDefault(x => x.Name.StartsWith(e.Name) && x.Name.Contains("GetDto")),
                    Key: e.GetProperty("Id")?.PropertyType
                ))
                .Where(x => x.Dto != null && x.Key != null)
                .ToList();

            foreach (var (entityType, dtoType, keyType) in pairs)
            {
                if (options.IgnoreEntities.Contains(entityType.Name))
                {
                    continue;
                }

                var type = typeof(AutoQueryTypeExtensions<,,>).MakeGenericType(dtoType, entityType, keyType);
                schema.AddTypeExtension((INamedTypeExtension)Activator.CreateInstance(type, options));
            }
        }
    }

    private static void AddGraphQLDelete(this IRequestExecutorBuilder schema, List<Assembly> assemblies, GraphQLOptions options)
    {
        if (options.Delete)
        {
            var entities = GetTypesImplementingInterfaces(assemblies, typeof(IEntity)).ToList();

            foreach (var entityType in entities)
            {
                var keyType = entityType.GetProperty("Id")?.PropertyType;
                if (keyType == null || options.IgnoreEntities.Contains(entityType.Name))
                {
                    continue;
                }

                var type = typeof(AutoDeleteMutationTypeExtensions<,>).MakeGenericType(entityType, keyType);
                schema.AddTypeExtension((INamedTypeExtension)Activator.CreateInstance(type, options));
            }
        }
    }

    private static void AddGraphQLCreate(this IRequestExecutorBuilder schema, List<Assembly> assemblies, GraphQLOptions options)
    {
        if (options.Create)
        {
            var entities = GetTypesImplementingInterfaces(assemblies, typeof(IEntity)).ToList();
            var dtos = GetTypesImplementingInterfaces(assemblies, typeof(IDto)).ToList();
            var inputs = GetTypesImplementingInterfaces(assemblies, typeof(IDto)).ToList();

            foreach (var entityType in entities)
            {
                var keyType = entityType.GetProperty("Id")?.PropertyType;
                if (keyType == null || options.IgnoreEntities.Contains(entityType.Name))
                {
                    continue;
                }

                var dtoType = dtos.FirstOrDefault(x => x.Name.StartsWith(entityType.Name) && x.Name.Contains("PostDto"));
                var inputType = inputs.FirstOrDefault(x => x.Name.StartsWith(entityType.Name) && x.Name.Contains("PostDto"));

                if (dtoType == null || inputType == null)
                {
                    continue;
                }

                var type = typeof(AutoCreateMutationTypeExtensions<,,,>).MakeGenericType(dtoType, entityType, keyType, inputType);
                schema.AddTypeExtension((INamedTypeExtension)Activator.CreateInstance(type, options));
            }
        }
    }

    private static void AddGraphQLUpdate(this IRequestExecutorBuilder schema, List<Assembly> assemblies, GraphQLOptions options)
    {
        if (options.Update)
        {
            var entities = GetTypesImplementingInterfaces(assemblies, typeof(IEntity)).ToList();
            var dtos = GetTypesImplementingInterfaces(assemblies, typeof(IDto)).ToList();
            var inputs = GetTypesImplementingInterfaces(assemblies, typeof(IDto)).ToList();

            foreach (var entityType in entities)
            {
                var keyType = entityType.GetProperty("Id")?.PropertyType;
                if (keyType == null || options.IgnoreEntities.Contains(entityType.Name))
                {
                    continue;
                }

                var dtoType = dtos.FirstOrDefault(x => x.Name.StartsWith(entityType.Name) && x.Name.Contains("PutDto"));
                var inputType = inputs.FirstOrDefault(x => x.Name.StartsWith(entityType.Name) && x.Name.Contains("PutDto"));

                if (dtoType == null || inputType == null)
                {
                    continue;
                }

                var type = typeof(AutoUpdateMutationTypeExtensions<,,,>).MakeGenericType(dtoType, entityType, keyType, inputType);
                schema.AddTypeExtension((INamedTypeExtension)Activator.CreateInstance(type, options));
            }
        }
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

    private static void Register(IServiceCollection services, Type entityType, Type idType)
    {
        var (handler, command) = GetTypes(entityType, idType);
        var (handlerById, commandById) = GetByIdTypes(entityType, idType);
        var getByIdsTypes = GetByIdsTypes(entityType, idType);
        var deleteTypes = DeleteTypes(entityType, idType);
        var patchTypes = PatchTypes(entityType, idType);

        services.AddScoped(deleteTypes.handler, deleteTypes.command);
        services.AddScoped(handlerById, commandById);
        services.AddScoped(getByIdsTypes.handler, getByIdsTypes.command);
        services.AddScoped(patchTypes.handler, patchTypes.command);
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

    private static (Type handler, Type command) PatchTypes(Type type, Type idType)
    {
        var requestType = typeof(PatchRequest<,>);
        var commandType = typeof(PatchCommand<,>);
        var responseType = typeof(BaseResponse<>);
        var handlerType = typeof(IRequestHandler<,>);

        var (handler, command) = MakeGenericTypeWith2Arguments(type, idType, requestType, commandType, responseType, handlerType);

        return (handler, command);
    }
}
