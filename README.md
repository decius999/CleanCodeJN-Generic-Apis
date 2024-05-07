# Generic Web Apis
### CRUD support for WebAPIs with the power of Mediator pattern, Automapper, DataRepositories and Entity Framework

> _This CleanCodeJN package uses ready to use build-in Minimal API Extensions to have basic 
CRUD operations with the ability to customize complex business logic by the power of the the 
Integration/Operation Segregation Principle using Commands and Repository Patterns._

### Features

- CRUD APIs (Minimal or Controller based) build in seconds
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
    public List<string> Tags => ["Customers V1"];

    public string Route => $"api/v1/Customers";

    public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>
    [
        app => app.MapGet<Customer, CustomerGetDto>(Route, Tags),
        app => app.MapGetById<Customer, CustomerGetDto>(Route, Tags),
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
    public List<string> Tags => ["Customers V1"];

    public string Route => $"api/v1/Customers";

    public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>
    [
         app => app.MapGet<Customer, CustomerGetDto>(Route, Tags, where: x => x.Name.StartsWith("a")),
    ];
}
```

__Or use ApiCrudControllerBase for CRUD operations in controllers__
```C#
[Tags("Customers V3")]
[Route($"api/v3/[controller]")]

public class CustomersController(IMediator commandBus, IMapper mapper)
    : ApiCrudControllerBase<Customer, CustomerGetDto, CustomerPostDto, CustomerPutDto>(commandBus, mapper)
{
}
```

__You can also override your Where and Include clauses__
```C#
[Tags("Customers V3")]
[Route($"api/v3/[controller]")]

public class CustomersController(IMediator commandBus, IMapper mapper)
    : ApiCrudControllerBase<Customer, CustomerGetDto, CustomerPostDto, CustomerPutDto>(commandBus, mapper)
{
    public override Expression<Func<Customer, bool>> GetWhere => x => x.Name.StartsWith("a");

    public override List<Expression<Func<Customer, object>>> GetIncludes => [x => x.Invoices];
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
public class SpecificDeleteCommand(IIntRepository<Customer> repository) : IRequestHandler<SpecificDeleteRequest, BaseResponse<Customer>>
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
