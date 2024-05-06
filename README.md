# Generic Web Apis
### CRUD support for (Minimal) WebAPIs with the power of Mediator pattern, Automapper, DataRepositories and Entity Framework

_This CleanCodeJN package uses build-in Command-Abstraction to use basic CRUD operations with the ability to customize complex 
business logic by the power of the the Integration/Operation Segregation Principle._

### Features

- CRUD APIs build in seconds
- Uses Mediator to abstract build-in and custom complex business logic
- Uses DataRepositories to abstract Entity Framework from business logic
- Enforces IOSP (Integration/Operation Segregation Principle) for commands
- Easy to mock and test
- On latest .NET 8.0

### How to use

- [Use CleanCodeJN Generic Repositories](https://www.nuget.org/packages/CleanCodeJN.Repository.EntityFramework/)
- Add RegisterRepositoriesCommandsWithAutomapper<IDataContext>() to your Program.cs
- Add app.RegisterApis() to your Program.cs
- Start writing Apis by implementing IApi
- Extend standard CRUD operations by specific Where() and Include() clauses
- Use IOSP for complex business logic

### Step by step explanation

__Add RegisterRepositoriesCommandsWithAutomapper<IDataContext>() to your Program.cs:__
```C#
builder.Services.RegisterRepositoriesCommandsWithAutomapper<MyDbContext>(cfg =>
{
    cfg.CreateMap<Customer, CustomerPutDto>().ReverseMap();
    cfg.CreateMap<Customer, CustomerPostDto>().ReverseMap();
    cfg.CreateMap<Customer, CustomerGetDto>().ReverseMap();
});
```

__Add app.RegisterApis() to your Program.cs:__
```C#
app.RegisterApis();
```

__Start writing Apis by implementing IApi:__
```C#
public class CustomersV1Api : IApi
{
    public List<string> Tags => ["Customers V1"];

    public string Route => $"api/v1/customers";

    public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>
    [
        app => app.MapGet<Customer, CustomerGetDto>(Route, Tags),
        app => app.MapGetById<Customer, CustomerGetDto>(Route, Tags),
        app => app.MapPut<Customer, CustomerPutDto, CustomerGetDto>(Route, Tags),
        app => app.MapPost<Customer, CustomerPostDto, CustomerGetDto>(Route, Tags),

        // Or use a custom Command with MapRequest
        app => app.MapDeleteRequest(Route, Tags, async (int id, [FromServices] ApiBase api) =>
                await api.Handle<Customer, CustomerGetDto>(new MyOwnDeleteRequest { Id = id }))
    ];
}
```

__Extend standard CRUD operations by specific Where() and Include() clauses__
```C#
public class CustomersV1Api : IApi
{
    public List<string> Tags => ["Customers V1"];

    public string Route => $"api/v1/customers";

    public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>
    [
         app => app.MapGet<Customer, CustomerGetDto>(Route, Tags, where: x => x.Name.StartsWith("a")),
    ];
}
```

__Use IOSP for complex business logic__

Derive from BaseIntegrationCommand:
```C#
public class YourIntegrationCommand(ICommandExecutionContext executionContext)
    : BaseIntegrationCommand(executionContext), IRequestHandler<YourIntegrationRequest, BaseResponse>
```

Write Extensions on ICommandExecutionContext with Built in Requests or with your own:
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

__See the how clean your code will look like at the end__
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

### Sample Code
[GitHub Full Sample](https://github.com/decius999/CleanCodeJN-Generic-Apis/tree/dev/CleanCodeJN.GenericApis.Sample)
