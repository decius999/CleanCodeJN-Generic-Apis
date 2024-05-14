# Generic Web Apis
### CRUD support for WebAPIs with the power of Mediator pattern, Automapper, DataRepositories and Entity Framework

> _This CleanCodeJN package streamlines the development of web APIs in .NET applications by providing a robust 
framework for CRUD operations and facilitating the implementation of complex business logic in a clean and maintainable manner._

### Features

- Paginated and filtered CRUD APIs (Minimal or Controller based) build in seconds
- Uses Mediator to abstract build-in and custom complex business logic
- Uses DataRepositories to abstract Entity Framework from business logic
- Enforces IOSP (Integration/Operation Segregation Principle) for commands
- Easy to mock and test
- On latest .NET 8.0

### How to use

- Add RegisterRepositoriesCommandsWithAutomapper<IDataContext>() to your Program.cs
- Add app.RegisterApis() to your Program.cs or use AddControllers + MapControllers()
- Start writing Apis by implementing IApi
- Extend standard CRUD operations by specific Where() and Include() clauses
- Use IOSP for complex business logic

# Step by step explanation

__Add RegisterRepositoriesCommandsWithAutomapper<IDataContext>() to your Program.cs__
```C#
builder.Services.RegisterRepositoriesCommandsWithAutomapper<MyDbContext>(cfg =>
{
    cfg.CreateMap<Customer, CustomerPutDto>().ReverseMap();
    cfg.CreateMap<Customer, CustomerPostDto>().ReverseMap();
    cfg.CreateMap<Customer, CustomerGetDto>().ReverseMap();
});
```

__Add app.RegisterApis() when using Minimal APIs to your Program.cs__
```C#
app.RegisterApis();
```

__When using Controllers add this to your Program.cs__
```C#
builder.Services.AddControllers();

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
         app => app.MapGet<Customer, CustomerGetDto, int>(Route, Tags,
            where: x => x.Name.StartsWith("a"), includes: [x => x.Invoices]),
        app => app.MapGetPaged<Customer, CustomerGetDto, int>(Route, Tags),
        app => app.MapGetFiltered<Customer, CustomerGetDto, int>(Route, Tags),
        app => app.MapGetById<Customer, CustomerGetDto, int>(Route, Tags),
        app => app.MapPut<Customer, CustomerPutDto, CustomerGetDto>(Route, Tags),
        app => app.MapPost<Customer, CustomerPostDto, CustomerGetDto>(Route, Tags),

        // Or use a custom Command with MapRequest
        app => app.MapDeleteRequest(Route, Tags, async (int id, [FromServices] ApiBase api) =>
                await api.Handle<Customer, CustomerGetDto>(new SpecificDeleteRequest { Id = id }))
    ];
}
```

__Extend standard CRUD operations by specific Where() and Include() clauses__
```C#
public class CustomersV1Api : IApi
{
    public List<string> Tags => ["Customers Minimal API"];

    public string Route => $"api/v1/Customers";

    public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>
    [
         app => app.MapGet<Customer, CustomerGetDto, int>(Route, Tags, where: x => x.Name.StartsWith("a")),
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

__You can also override your Where and Include clauses__
```C#
[Tags("Customers Controller based")]
[Route($"api/v2/[controller]")]

public class CustomersController(IMediator commandBus, IMapper mapper)
    : ApiCrudControllerBase<Customer, CustomerGetDto, CustomerPostDto, CustomerPutDto, int>(commandBus, mapper)
{
    public override Expression<Func<Customer, bool>> GetWhere => x => x.Name.StartsWith("a");

    public override List<Expression<Func<Customer, object>>> GetIncludes => [x => x.Invoices];
}
```

__For using the /filtered api with a filter, just provide a serialized json as filter parameter, like this:__
```C#
{
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
}
```

# More Advanced Topics
__Implement your own specific Request:__
```C#
public class SpecificDeleteRequest : IRequest<BaseResponse<Customer>>
{
    public required int Id { get; init; }
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
    : BaseIntegrationCommand(executionContext), IRequestHandler<YourIntegrationRequest, BaseResponse>
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
    : BaseIntegrationCommand(executionContext), IRequestHandler<YourIntegrationRequest, BaseResponse>
{
    public async Task<BaseResponse> Handle(YourIntegrationRequest request, CancellationToken cancellationToken) =>
        await ExecutionContext
            .CandidateGetByIdRequest(request.Dto.CandidateId)
            .CustomerGetByIdRequest(request.Dto.CustomerIds)
            .GetOtherStuffRequest(request.Dto.XYZType)
            .PostSomethingRequest(request.Dto)
            .SendMailRequest()
            .Execute(cancellationToken);
}
```

# Sample Code
[GitHub Full Sample](https://github.com/decius999/CleanCodeJN-Generic-Apis/tree/dev/CleanCodeJN.GenericApis.Sample)
