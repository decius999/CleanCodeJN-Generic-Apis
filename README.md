# Generic Web Apis
### CRUD support for WebAPIs with the power of Mediator pattern, Automapper with automatic mapping, DataRepositories and Entity Framework

> _This CleanCodeJN package streamlines the development of web APIs in .NET applications by providing a robust 
framework for CRUD operations and facilitating the implementation of complex business logic in a clean and maintainable manner._

### Features

- Paginated and filtered CRUD APIs (Minimal or Controller based) build in seconds
- Uses Mediator to abstract build-in and custom complex business logic
- Uses DataRepositories to abstract Entity Framework from business logic
- Enforces IOSP (Integration/Operation Segregation Principle) for commands
- Easy to mock and test
- Automatic Entity to DTO mapping (no mapping config needed)
- Built-in support for Fluent Validation
- On latest .NET 8.0

### How to use

- Add AddCleanCodeJN<IDataContext>() to your Program.cs
- Add app.UseCleanCodeJNWithMinimalApis() to your Program.cs for minimal APIs or use AddControllers + MapControllers()
- Start writing Apis by implementing IApi
- Extend standard CRUD operations by specific Where() and Include() clauses
- Use IOSP for complex business logic

# Step by step explanation

__Add AddCleanCodeJN<IDataContext>() to your Program.cs__
```C#
// All Entity <=> DTO Mappings will be done automatically if the naming Convention will be applied:
// e.g.: Customer <=> CustomerGetDto 
// DTO has to start with Entity-Name and must inherits from IDto
// Entity must inherit from IEntity
builder.Services.AddCleanCodeJN<MyDbContext>(options => {});
```

__These are the CleanCodeJN Options__
```C#
/// <summary>
/// The options for the CleanCodeJN.GenericApis
/// </summary>
public class CleanCodeOptions
{
    /// <summary>
    /// The assemblies that contain the command types, Entity types and DTO types for automatic registration of commands, DTOs and entities.
    /// </summary>
    public List<Assembly> ApplicationAssemblies { get; set; } = [];

    /// <summary>
    /// The assembly that contains the validators types for using Fluent Validation.
    /// </summary>
    public Assembly ValidatorAssembly { get; set; }

    /// <summary>
    /// The assembly that contains the automapper mapping profiles.
    /// </summary>
    public Action<IMapperConfigurationExpression> MappingOverrides { get; set; }

    /// <summary>
    /// If true: Use distributed memory cache. If false: you can add another Distributed Cache implementation.
    /// </summary>
    public bool UseDistributedMemoryCache { get; set; } = true;

    /// <summary>
    /// If true: Add default logging behavior. If false: you can add another logging behavior.
    /// </summary>
    public bool AddDefaultLoggingBehavior { get; set; }

    /// <summary>
    /// Mediatr Types of Open Behaviors to register
    /// </summary>
    public List<Type> OpenBehaviors { get; set; } = [];

    /// <summary>
    /// Mediatr Types of Closed Behaviors to register
    /// </summary>
    public List<Type> ClosedBehaviors { get; set; } = [];
}
```

__Add app.UseCleanCodeJNWithMinimalApis() when using Minimal APIs to your Program.cs__
```C#
app.UseCleanCodeJNWithMinimalApis();
```

__When using Controllers add this to your Program.cs__
```C#
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // this is needed for "http patch" only. If you do not need to use patch, you can remove this line

// After Build()
app.MapControllers();
```

__Start writing Minimal Apis by implementing IApi__
```C#
public class CustomersV1Api : IApi
{
    public List<string> Tags => ["Customers Minimal API"];

    public string Route => $"api/v1/Customers";

    public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>
    [
        app => app.MapGet<Customer, CustomerGetDto, int>(
            Route,
            Tags,
            where: x => x.Name.StartsWith("Customer"),
            includes: [x => x.Invoices],
            select: x => new Customer { Name = x.Name },
            ignoreQueryFilters: true),

        app => app.MapGetPaged<Customer, CustomerGetDto, int>(Route, Tags),

        app => app.MapGetFiltered<Customer, CustomerGetDto, int>(Route, Tags),

        app => app.MapGetById<Customer, CustomerGetDto, int>(Route, Tags),

        app => app.MapPut<Customer, CustomerPutDto, CustomerGetDto>(Route, Tags),

        app => app.MapPost<Customer, CustomerPostDto, CustomerGetDto>(Route, Tags),

        app => app.MapPatch<Customer, CustomerGetDto, int>(Route, Tags),

        // Or use a custom Command with MapDeleteRequest()
        app => app.MapDeleteRequest<Customer, CustomerGetDto, int>(Route, Tags, id => new DeleteCustomerIntegrationRequest { Id = id })
    ];
}
```

__Extend standard CRUD operations by specific Where(), Include() or Select() clauses__
```C#
public class CustomersV1Api : IApi
{
    public List<string> Tags => ["Customers Minimal API"];

    public string Route => $"api/v1/Customers";

    public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>
    [
         app => app.MapGet<Customer, CustomerGetDto, int>(Route, Tags, where: x => x.Name.StartsWith("a"), select: x => new Customer { Name = x.Name }),
    ];
}
```

__Or use ApiCrudControllerBase for CRUD operations in controllers__
```C#
[Tags("Customers Controller based")]
[Route($"api/v2/[controller]")]

public class CustomersController(IMediator commandBus, IMapper mapper)
    : ApiCrudControllerBase<Customer, CustomerGetDto, CustomerPostDto, CustomerPutDto, int>(commandBus, mapper)
{
}
```

__You can also override your Where, Include or Select clauses__
```C#
[Tags("Customers Controller based")]
[Route($"api/v2/[controller]")]

public class CustomersController(IMediator commandBus, IMapper mapper)
    : ApiCrudControllerBase<Customer, CustomerGetDto, CustomerPostDto, CustomerPutDto, int>(commandBus, mapper)
{
    public override Expression<Func<Customer, bool>> GetWhere => x => x.Name.StartsWith("a");

    public override List<Expression<Func<Customer, object>>> GetIncludes => [x => x.Invoices];

    public override Expression<Func<Customer, Customer>> GetSelect => x => new Customer { Name = x.Name };
}
```

__For using the /filtered api with a filter, just provide a serialized json as filter parameter, like this:__
```C#
{
    "Condition" : 0, // 0 = AND; 1 = OR
    "Filters": [
        {
            "Field": "Name",
            "Value": "aac",
            "Type": 0
        },
        {
            "Field": "Id",
            "Value": "3",
            "Type": 1
        }
    ]
}
```

>Which means: Give me all Names which CONTAINS "aac" AND have Id EQUALS 3. So string Types use always CONTAINS and integer types use EQUALS. All filters are combined with ANDs.

__The Type can be specified with these values:__
```C#
public enum FilterTypeEnum
{
    STRING = 0,
    INTEGER = 1,
    DOUBLE = 2,
    INTEGER_NULLABLE = 3,
    DOUBLE_NULLABLE = 4,
    DATETIME = 5,
    DATETIME_NULLABLE = 6,
    GUID = 7,
    GUID_NULLABLE = 8,
}
```

# More Advanced Topics
__Built-in Support for Fluent Validation:__

Just write your AbstractValidators<T>. They will be automatically executed on generic POST and generic PUT actions:

```C#
public class CustomerPostDtoValidator : AbstractValidator<CustomerPostDto>
{
    public CustomerPostDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(10);
    }
```


```C#
public class CustomerPutDtoValidator : AbstractValidator<CustomerPutDto>
{
    public CustomerPutDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(10)
            .CreditCard();
    }
}
```

__Implement your own specific Request:__
```C#
public class SpecificDeleteRequest : IRequest<BaseResponse<Customer>>
{
    public required int Id { get; init; }
}
```

__Requests can also be marked as ICachableRequest, which uses IDistributedCache to cache the Response:__
```C#
public class SpecificDeleteRequest : IRequest<BaseResponse<Customer>>, ICachableRequest
{
    public required int Id { get; init; }

    public bool BypassCache { get; }

    public string CacheKey => "Your Key";

    public TimeSpan? CacheDuration => TimeSpan.FromHours(168);
}
```

__With your own specific Command using CleanCodeJN.Repository__
```C#
public class SpecificDeleteCommand(IRepository<Customer, int> repository) : IRequestHandler<SpecificDeleteRequest, BaseResponse<Customer>>
{
    public async Task<BaseResponse<Customer>> Handle(SpecificDeleteRequest request, CancellationToken cancellationToken)
    {
        var deletedCustomer = await repository.Delete(request.Id, cancellationToken);

        return await BaseResponse<Customer>.Create(deletedCustomer is not null, deletedCustomer);
    }
}
```

__Use IOSP for complex business logic__

Derive from BaseIntegrationCommand:
```C#
public class YourIntegrationCommand(ICommandExecutionContext executionContext)
    : IntegrationCommand<YourIntegrationRequest, YourDomainObject>(executionContext)
```

Write Extensions on ICommandExecutionContext with Built in Requests or with your own
```C#
public static ICommandExecutionContext CustomerGetByIdRequest(
    this ICommandExecutionContext executionContext, int customerId) 
    => executionContext.WithRequest(
            () => new GetByIdRequest<Customer>
            {
                Id = customerId,
                Includes = [x => x.Invoices, x => x.OtherDependentTable],
            },
            CommandConstants.CustomerGetById);
```

__See the how clean your code will look like in the end__
```C#
public class YourIntegrationCommand(ICommandExecutionContext executionContext)
    : IntegrationCommand<YourIntegrationRequest, Customer>(executionContext)
{
    public override async Task<BaseResponse<Customer>> Handle(YourIntegrationRequest request, CancellationToken cancellationToken) =>
        await ExecutionContext
            .CandidateGetByIdRequest(request.Dto.CandidateId)
            .CustomerGetByIdRequest(request.Dto.CustomerIds)
            .GetOtherStuffRequest(request.Dto.XYZType)
            .PostSomethingRequest(request.Dto)
            .SendMailRequest()
            .Execute<Customer>(cancellationToken);
}
```

# Sample Code
[GitHub Full Sample](https://github.com/decius999/CleanCodeJN-Generic-Apis/tree/dev/CleanCodeJN.GenericApis.Sample)
