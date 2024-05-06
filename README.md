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

_Add RegisterRepositoriesCommandsWithAutomapper<IDataContext>() to your Program.cs:_
```C#
builder.Services.RegisterRepositoriesCommandsWithAutomapper<MyDbContext>(cfg =>
{
    cfg.CreateMap<Customer, CustomerPutDto>().ReverseMap();
    cfg.CreateMap<Customer, CustomerPostDto>().ReverseMap();
    cfg.CreateMap<Customer, CustomerGetDto>().ReverseMap();
});
```

_Add app.RegisterApis() to your Program.cs:_
```C#
app.RegisterApis();
```

_Start writing Apis by implementing IApi:_
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
        // Or use a custom Command
        app => app.MapDeleteRequest(Route, Tags, async (int id, [FromServices] ApiBase api) =>
                await api.Handle<Customer, CustomerGetDto>(new DeleteRequest<Customer> { Id = id }))
    ];
}
```

_Extend standard CRUD operations by specific Where() and Include() clauses_
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

### Sample Code
[GitHub Full Sample](https://github.com/decius999/CleanCodeJN-Generic-Apis/tree/dev/CleanCodeJN.GenericApis.Sample)
