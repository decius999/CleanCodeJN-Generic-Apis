# 🚀 Generic Web APIs – Fast, Clean, Powerful

> _Create clean and testable web APIs in seconds – from **simple CRUD** to fully structured **IOSP-based** architectures – 
> powered by the **Mediator pattern**, **AutoMapper**, **FluentValidation** and **Entity Framework**, 
> all wired together with Clean Code principles._

## Table of Contents

- [🚀 Generic Web APIs – Fast, Clean, Powerful](#🚀-generic-web-apis-–-fast,-clean,-powerful)
  - [What is in this package](#this-package-gives-you:)
  - [🧪 What is IOSP?](#🧪-what-is-iosp?)
  - [✨ Features at a Glance](#✨-features-at-a-glance)
  - [How to use](#how-to-use)
- [Step by step explanation](#step-by-step-explanation)
    - [Add AddCleanCodeJN<IDataContext>() to your Program.cs](#add-addcleancodejn<idatacontext>()-to-your-program.cs)
    - [These are the CleanCodeJN Options](#these-are-the-cleancodejn-options)
    - [Add app.UseCleanCodeJNWithMinimalApis() when using Minimal APIs to your Program.cs](#add-app.usecleancodejnwithminimalapis()-when-using-minimal-apis-to-your-program.cs)
    - [When using Controllers add this to your Program.cs](#when-using-controllers-add-this-to-your-program.cs)
    - [Start writing Minimal Apis by implementing IApi](#start-writing-minimal-apis-by-implementing-iapi)
    - [Extend standard CRUD operations by specific Where(), Include() or Select() clauses](#extend-standard-crud-operations-by-specific-where(),-include()-or-select()-clauses)
    - [Use ApiCrudControllerBase for CRUD operations in controllers](#use-apicrudcontrollerbase-for-crud-operations-in-controllers)
    - [You can also override your Where, Include or Select clauses](#you-can-also-override-your-where,-include-or-select-clauses)
    - [For using the /filtered api with a filter, just provide a serialized json as filter parameter](#for-using-the-/filtered-api-with-a-filter,-just-provide-a-serialized-json-as-filter-parameter,-like-this:)
    - [The Type can be specified with these values](#the-type-can-be-specified-with-these-values)
- [Advanced Topics](#advanced-topics)
    - [Built-in Support for Fluent Validation](#built-in-support-for-fluent-validation:)
    - [Implement your own specific Request](#implement-your-own-specific-request:)
    - [Requests can also be marked as ICachableRequest, which uses IDistributedCache to cache the Response](#requests-can-also-be-marked-as-icachablerequest,-which-uses-idistributedcache-to-cache-the-response:)
    - [With your own specific Command using CleanCodeJN.Repository](#with-your-own-specific-command-using-cleancodejn.repository)
  - [Use IOSP for complex business logic](#use-iosp-for-complex-business-logic)
    - [Derive from BaseIntegrationCommand](#derive-from-baseintegrationcommand:)
    - [Write Extensions on ICommandExecutionContext with Built in Requests or with your own](#write-extensions-on-icommandexecutioncontext-with-built-in-requests-or-with-your-own)
    - [Use WithParallelWhenAllRequests() to execute multiple requests in parallel and execute when all tasks are finished](#use-withparallelwhenallrequests()-to-execute-multiple-requests-in-parallel-and-execute-when-all-tasks-are-finished:)
    - [Use GetListParallelWhenAll() to get all results of WithParallelWhenAllRequests](#use-getListParallelWhenAll()-to-get-all-results-of-withParallelWhenAllRequests():)
    - [Use GetParallelWhenAllByIndex<T> to get the result of the WithParallelWhenAllRequests with a typed object by index](#use-getParallelWhenAllByIndex<T>()-to-get-the-result-of-the-withParallelWhenAllRequests()-with-a-typed-object-by-index:)
    - [Use IfRequest() to execute an optional request - continue when conditions are not satisfied](#use-ifrequest()-to-execute-an-optional-request---continue-when-conditions-are-not-satisfied:)
    - [Use IfBreakRequest() to execute an optional request - break whole process when conditions are not satisfied](#use-ifbreakrequest()-to-execute-an-optional-request---break-whole-process-when-conditions-are-not-satisfied:)
    - [See the how clean your code will look like in the end](#see-the-how-clean-your-code-will-look-like-in-the-end)
- [Sample Code](#sample-code)


## This package gives you:  
- ✅ blazing-fast setup
- ✅ CRUD APIs in seconds without writing a single line of code
- ✅ complex business logic with **IOSP**
- ✅ testable architecture  
- ✅ maintainable and modular code

## 🧪 What is IOSP?

> **Integration Operation Segregation Principle**  
> Split your request handlers into:
> **Operations** → contain real logic (DB, API, etc.)  
> **Integrations** → just orchestrate other request handlers  

## ✨ Features at a Glance
- ⚡ **Plug & Play CRUD APIs** (Minimal API or Controller-based)
- 📦 **Built-in paging, filtering & projections**
- 🧠 **Clean separation of logic** using the **Mediator pattern**
- 🧱 **Entity Framework abstraction** via `DataRepositories`
- 🔀 **Auto-mapping** of Entities ⇄ DTOs (no config needed)
- 🧪 **Fluent validation** out of the box
- 🧼 **CleanCode-first architecture** using the **IOSP Principle**  
- 🧪 **Easily mockable and testable**  
- 🚀 Runs on **.NET 9**

## How to use

- Add AddCleanCodeJN<IDataContext>() to your Program.cs
- Add app.UseCleanCodeJNWithMinimalApis() to your Program.cs for minimal APIs or use AddControllers + MapControllers()
- Start writing Apis by implementing IApi
- Extend standard CRUD operations by specific Where() and Include() clauses
- Use IOSP for complex business logic

# Step by step explanation

### Add AddCleanCodeJN<IDataContext>() to your Program.cs
```C#
builder.Services.AddCleanCodeJN<MyDbContext>(options => {});
```
- All Entity <=> DTO Mappings will be done automatically if the naming Convention will be applied: e.g.: Customer <=> CustomerGetDto.
- DTO has to start with Entity-Name and must inherits from IDto
- Entity must inherit from IEntity

### These are the CleanCodeJN Options
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

### Add app.UseCleanCodeJNWithMinimalApis() when using Minimal APIs to your Program.cs
```C#
app.UseCleanCodeJNWithMinimalApis();
```

### When using Controllers add this to your Program.cs
```C#
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // this is needed for "http patch" only. If you do not need to use patch, you can remove this line

// After Build()
app.MapControllers();
```

### Start writing Minimal Apis by implementing IApi
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

### Extend standard CRUD operations by specific Where(), Include() or Select() clauses
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

### Use ApiCrudControllerBase for CRUD operations in controllers
```C#
[Tags("Customers Controller based")]
[Route($"api/v2/[controller]")]

public class CustomersController(IMediator commandBus, IMapper mapper)
    : ApiCrudControllerBase<Customer, CustomerGetDto, CustomerPostDto, CustomerPutDto, int>(commandBus, mapper)
{
}
```

### You can also override your Where, Include or Select clauses
```C#
/// <summary>
/// Customers Controller based
/// </summary>
/// <param name="commandBus">IMediatr instance.</param>
/// <param name="mapper">Automapper instance.</param>
[Tags("Customers Controller based")]
[Route($"api/v2/[controller]")]
public class CustomersController(IMediator commandBus, IMapper mapper)
    : ApiCrudControllerBase<Customer, CustomerGetDto, CustomerPostDto, CustomerPutDto, int>(commandBus, mapper)
{
    /// <summary>
    /// Where clause for the Get method.
    /// </summary>
    public override Expression<Func<Customer, bool>> GetWhere => x => x.Name.StartsWith("Customer");

    /// <summary>
    /// Includes for the Get method.
    /// </summary>
    public override List<Expression<Func<Customer, object>>> GetIncludes => [x => x.Invoices];

    /// <summary>
    /// Select for the Get method.
    /// </summary>
    public override Expression<Func<Customer, Customer>> GetSelect => x => new Customer { Id = x.Id, Name = x.Name };

    /// <summary>
    /// AsNoTracking for the Get method.
    /// </summary>
    public override bool AsNoTracking => true;
}
```

### For using the /filtered api with a filter, just provide a serialized json as filter parameter, like this:
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

### The Type can be specified with these values
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

# Advanced Topics
### Built-in Support for Fluent Validation:

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

### Implement your own specific Request:
```C#
public class SpecificDeleteRequest : IRequest<BaseResponse<Customer>>
{
    public required int Id { get; init; }
}
```

### Requests can also be marked as ICachableRequest, which uses IDistributedCache to cache the Response:
```C#
public class SpecificDeleteRequest : IRequest<BaseResponse<Customer>>, ICachableRequest
{
    public required int Id { get; init; }

    public bool BypassCache { get; }

    public string CacheKey => "Your Key";

    public TimeSpan? CacheDuration => TimeSpan.FromHours(168);
}
```

### With your own specific Command using CleanCodeJN.Repository
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

## Use IOSP for complex business logic

### Derive from BaseIntegrationCommand:
```C#
public class YourIntegrationCommand(ICommandExecutionContext executionContext)
    : IntegrationCommand<YourIntegrationRequest, YourDomainObject>(executionContext)
```

### Write Extensions on ICommandExecutionContext with Built in Requests or with your own
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

### Use WithParallelWhenAllRequests() to execute multiple requests in parallel and execute when all tasks are finished:
```C#
   executionContext.WithParallelWhenAllRequests(
                [
                    () => new GetByIdRequest<Customer, int>
                          {
                              Id = request.Id,
                          },
                    () => new GetByIdRequest<Customer, int>
                          {
                              Id = request.Id,
                          },
                ])
```

### Use GetListParallelWhenAll() to get all results of WithParallelWhenAllRequests():
```C#
   .WithRequest(
                () => new YourSpecificRequest
                {
                    Results = executionContext.GetListParallelWhenAll("Parallel Block"),
                })
```

### Use GetParallelWhenAllByIndex<T>() to get the result of the WithParallelWhenAllRequests() with a typed object by index:
```C#
   .WithRequest(
                () => new GetByIdRequest<Invoice, Guid>
                {
                    Id = executionContext.GetParallelWhenAllByIndex<Invoice>("Parallel Block", 1).Id,
                })
```


### Use IfRequest() to execute an optional request - continue when conditions are not satisfied:
```C#
    executionContext.IfRequest(() => new GetByIdRequest<Customer, int> { Id = request.Id },
                               ifBeforePredicate: () => true,
                               ifAfterPredicate: response => response.Succeeded)
```

### Use IfBreakRequest() to execute an optional request - break whole process when conditions are not satisfied:
```C#
    executionContext.IfBreakRequest(() => new GetByIdRequest<Customer, int> { Id = request.Id },
                                    ifBeforePredicate: () => true,
                                    ifAfterPredicate: response => response.Succeeded)
```

### See the how clean your code will look like in the end
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
