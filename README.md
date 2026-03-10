# 🚀 Generic Web APIs – Fast, Clean, Powerful

> _Build **production-ready APIs** instantly – from **Minimal APIs** and **Controllers** to fully integrated **GraphQL endpoints** and an **AI-ready MCP Server** with
**CRUD**, **filtering**, **sorting** & **paging** – powered by **Mediator**, **AutoMapper**, **EF Core**, **FluentValidation**, and the
**IOSP architecture pattern**._

## ⚡ Getting Started

```bash
dotnet add package CleanCodeJN.GenericApis
```

```csharp
// Program.cs
builder.Services.AddCleanCodeJN<MyDbContext>(options =>
{
    options.ApplicationAssemblies =           // assemblies with your Commands, DTOs, Entities
    [
        typeof(YourBusiness.AssemblyRegistration).Assembly,
        typeof(YourCore.AssemblyRegistration).Assembly,
    ];
    options.ValidatorAssembly = typeof(YourCore.AssemblyRegistration).Assembly;
    options.AddDefaultLoggingBehavior = true; // optional
});

var app = builder.Build();

app.UseCleanCodeJNWithMinimalApis();   // REST: registers all IApi endpoints        → /api/...
app.UseCleanCodeJNWithGraphQL();       // GraphQL: auto-schema from entities/DTOs   → /graphql
app.UseCleanCodeJNWithMcp();           // MCP Server: every endpoint = AI tool      → /mcp
app.UseCleanCodeJNWithDocumentation(); // IOSP command docs from XML comments       → /docs
app.UseCleanCodeJNWithAiChat();        // AI chat with your API based on MCP server → /ai
app.MapControllers();

app.Run();
```

```csharp
// Entity + DTO (naming convention: <EntityName>GetDto / PostDto / PutDto)
public class Customer : IEntity<int> { public int Id { get; set; } public string Name { get; set; } }
public class CustomerGetDto : IDto   { public int Id { get; set; } public string Name { get; set; } }

// Minimal API — full CRUD in ~10 lines
public class CustomersApi : IApi
{
    public List<string> Tags => ["Customers"];
    public string Route => "api/v1/Customers";
    public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>
    [
        app => app.MapGet<Customer, CustomerGetDto, int>(Route, Tags),
        app => app.MapGetById<Customer, CustomerGetDto, int>(Route, Tags),
        app => app.MapPost<Customer, CustomerPostDto, CustomerGetDto>(Route, Tags),
        app => app.MapPut<Customer, CustomerPutDto, CustomerGetDto>(Route, Tags),
        app => app.MapPatch<Customer, CustomerGetDto, int>(Route, Tags),
        app => app.MapDeleteRequest<Customer, CustomerGetDto, int>(Route, Tags, id => new DeleteCustomerRequest { Id = id }),
    ];
}

// IOSP: complex business logic as clean orchestration
public class DeleteCustomerCommand(ICommandExecutionContext ctx)
    : IntegrationCommand<DeleteCustomerRequest, Customer>(ctx)
{
    public override async Task<BaseResponse<Customer>> Handle(DeleteCustomerRequest request, CancellationToken ct) =>
        await ExecutionContext
            .GetCustomerByIdRequest(request.Id)
            .ValidateInvoicesRequest()
            .DeleteCustomerRequest()
            .Execute<Customer>(ct);
}
```

### 3. `Program.cs` — AI Proxy config (only if using `/ai`)

```csharp
    options.AiProxyOptions = new AiProxyOptions
    {
        AnthropicApiKey = configuration["Anthropic:ApiKey"],
        SelfBaseUrl = configuration["SelfBaseUrl"],
        Model = "claude-sonnet-4-6",
        MaxTokens = 4096,
    };
```

### 4. The three building blocks

| Block | What you write | What you get |
|---|---|---|
| **Entity** | `class Customer : IEntity<int>` | EF Core + Repository + GraphQL type |
| **DTO** | `class CustomerGetDto : IDto` | Auto-mapped, Swagger schema, MCP output schema |
| **IApi** | `class CustomersApi : IApi` | REST endpoints + MCP tools + GraphQL queries |

```csharp
// Entity
public class Customer : IEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// DTO (naming convention: <EntityName>GetDto / PostDto / PutDto)
public class CustomerGetDto : IDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// Minimal API — all CRUD in ~10 lines
public class CustomersApi : IApi
{
    public List<string> Tags => ["Customers"];
    public string Route => "api/v1/Customers";

    public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>
    [
        app => app.MapGet<Customer, CustomerGetDto, int>(Route, Tags),
        app => app.MapGetById<Customer, CustomerGetDto, int>(Route, Tags),
        app => app.MapPost<Customer, CustomerPostDto, CustomerGetDto>(Route, Tags),
        app => app.MapPut<Customer, CustomerPutDto, CustomerGetDto>(Route, Tags),
        app => app.MapPatch<Customer, CustomerGetDto, int>(Route, Tags),
        app => app.MapDeleteRequest<Customer, CustomerGetDto, int>(Route, Tags, id => new DeleteCustomerRequest { Id = id }),
    ];
}
```

### 5. Feature flags — what each `Use*` call activates

| Call | Activates | Visit |
|---|---|---|
| `UseCleanCodeJNWithMinimalApis()` | All `IApi` REST endpoints | `/api` + /swagger |
| `UseCleanCodeJNWithGraphQL()` | Auto-generated GraphQL schema | `/graphql` |
| `UseCleanCodeJNWithMcp()` | MCP Server (AI-callable tools) | `/mcp` |
| `UseCleanCodeJNWithDocumentation()` | Automatic documentation from XML comments | `/docs` |
| `UseCleanCodeJNWithAiChat()` | AI chat with your API based on MCP server | `/ai` |
---

## Table of Contents

- [⚡ Getting Started](#-getting-started)
- [🚀 Generic Web APIs – Fast, Clean, Powerful](#🚀-generic-web-apis-–-fast,-clean,-powerful)
  - [✨ This package gives you](#-this-package-gives-you)
  - [🧪 What is IOSP?](#🧪-what-is-iosp?)
- [Step by step explanation](#step-by-step-explanation)
    - [Add AddCleanCodeJN\<IDataContext>() to your Program.cs](#add-addcleancodejnidatacontext-to-your-programcs)
    - [These are the CleanCodeJN Options](#these-are-the-cleancodejn-options)
    - [Add app.UseCleanCodeJNWithMinimalApis() when using Minimal APIs to your Program.cs](#add-appusecleancodejnwithminimalapis-when-using-minimal-apis-to-your-programcs)
    - [Add app.UseCleanCodeJNWithGraphQL() when using automatic GraphQL to your Program.cs](#add-appusecleancodejnwithgraphql-when-using-automatic-graphql-to-your-programcs)
    - [Add app.UseCleanCodeJNDocumentation() when using automatic Command documentation (from your XML comments) to your Program.cs](#add-appusecleancodejndocumentation-when-using-automatic-command-documentation-from-your-xml-comments-to-your-programcs)
    - [🤖 Add app.UseCleanCodeJNWithMcp() to expose your API as an AI-callable MCP Server](#-add-appusecleancodejnwithmcp-to-expose-your-api-as-an-ai-callable-mcp-server)
    - [When using Controllers add this to your Program.cs](#when-using-controllers-add-this-to-your-programcs)
    - [Start writing Minimal Apis by implementing IApi](#start-writing-minimal-apis-by-implementing-iapi)
    - [Extend standard CRUD operations by specific Where(), Include() or Select() clauses](#extend-standard-crud-operations-by-specific-where-include-or-select-clauses)
    - [Use ApiCrudControllerBase for CRUD operations in controllers](#use-apicrudcontrollerbase-for-crud-operations-in-controllers)
    - [You can also override your Where, Include or Select clauses](#you-can-also-override-your-where-include-or-select-clauses)
    - [For using the /filtered api with a filter, just provide a serialized json as filter parameter](#for-using-the-filtered-api-with-a-filter-just-provide-a-serialized-json-as-filter-parameter-like-this)
    - [The Type can be specified with these values](#the-type-can-be-specified-with-these-values)
- [Advanced Topics](#advanced-topics)
    - [Built-in Support for Fluent Validation](#built-in-support-for-fluent-validation)
    - [Implement your own specific Request](#implement-your-own-specific-request)
    - [Requests can also be marked as ICachableRequest, which uses IDistributedCache to cache the Response](#requests-can-also-be-marked-as-icachablerequest-which-uses-idistributedcache-to-cache-the-response)
    - [With your own specific Command using CleanCodeJN.Repository](#with-your-own-specific-command-using-cleancodejnrepository)
    - [Custom Middlewares](#custom-middlewares)
  - [Use IOSP for complex business logic](#use-iosp-for-complex-business-logic)
    - [Derive from BaseIntegrationCommand](#derive-from-baseintegrationcommand)
    - [Write Extensions on ICommandExecutionContext with Built in Requests or with your own](#write-extensions-on-icommandexecutioncontext-with-built-in-requests-or-with-your-own)
    - [Use WithParallelWhenAllRequests() to execute multiple requests in parallel and execute when all tasks are finished](#use-withparallelwhenallrequests-to-execute-multiple-requests-in-parallel-and-execute-when-all-tasks-are-finished)
    - [Use GetListParallelWhenAll() to get all results of WithParallelWhenAllRequests](#use-getlistparallelwhenall-to-get-all-results-of-withparallelwhenallrequests)
    - [Use GetParallelWhenAllByIndex\<T> to get the result of the WithParallelWhenAllRequests with a typed object by index](#use-getparallelwhenallbyindext-to-get-the-result-of-the-withparallelwhenallrequests-with-a-typed-object-by-index)
    - [Use IfRequest() to execute an optional request - continue when conditions are not satisfied](#use-ifrequest-to-execute-an-optional-request---continue-when-conditions-are-not-satisfied)
    - [Use IfBreakRequest() to execute an optional request - break whole process when conditions are not satisfied](#use-ifbreakrequest-to-execute-an-optional-request---break-whole-process-when-conditions-are-not-satisfied)
    - [See the how clean your code will look like in the end](#see-the-how-clean-your-code-will-look-like-in-the-end)
- [💬 AI Chat UI — /ai Page](#-ai-chat-ui--ai-page)
- [Sample Code](#sample-code)


## ✨ This package gives you

- ⚡ **CRUD APIs in seconds** — Minimal API or Controller-based, zero boilerplate
- 🧬 **Auto-generated GraphQL** — query/mutation/filter/sort/projection via HotChocolate
- 🤖 **MCP Server** — one line exposes your entire API as AI-callable tools (Claude, Cursor, …)
- 💬 **AI Chat UI** — ready-made `/ai` Blazor page: chat with your API in natural language
- 📦 **Paging, filtering & projections** — built-in, no extra code
- 🔀 **Auto-mapping** — Entities ⇄ DTOs by naming convention, no AutoMapper config needed
- 🧪 **FluentValidation** — validators auto-discovered and executed on POST/PUT
- 🧼 **IOSP architecture** — clean orchestration of complex business logic
- 📄 **Command docs** — auto-generated workflow documentation from XML comments at `/docs`
- 🚀 **.NET 10**, EF Core 10, fully testable & mockable

## 🧪 What is IOSP?

> **Integration Operation Segregation Principle** — split your handlers into:
> **Operations** → real logic (DB, external APIs, …)
> **Integrations** → pure orchestration of other handlers, no logic of their own


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
    
    /// <summary>
    /// Gets or sets a value indicating whether GraphQL auto-wiring is enabled.
    /// </summary>
    public GraphQLOptions GraphQLOptions { get; set; }
}
```

### Add app.UseCleanCodeJNWithMinimalApis() when using Minimal APIs to your Program.cs
```C#
app.UseCleanCodeJNWithMinimalApis();
```

### Add app.UseCleanCodeJNWithGraphQL() when using automatic GraphQL to your Program.cs
```C#
app.UseCleanCodeJNWithGraphQL();
```

### Add app.UseCleanCodeJNDocumentation() when using automatic Command documentation (from your XML comments) to your Program.cs
```C#
app.UseCleanCodeJNDocumentation();  // add <GenerateDocumentationFile>true</GenerateDocumentationFile> to your .csproj file
```

---

### 🤖 Add app.UseCleanCodeJNWithMcp() to expose your API as an AI-callable MCP Server

One line of code turns your entire API into a **Model Context Protocol (MCP) Server** — discoverable and executable by any AI assistant that supports the standard MCP Streamable HTTP transport (Claude, Cursor, Continue, and more).

```C#
app.UseCleanCodeJNWithMcp();
```

This registers a `POST /mcp` endpoint implementing the **MCP Streamable HTTP transport** (protocol version `2024-11-05`). No custom protocol, no vendor lock-in — any standard MCP client works out of the box.

#### What gets auto-generated?

**CRUD tools** are generated automatically from your Controllers and Minimal APIs — including full JSON schemas derived from your DTOs:

| Tool | Description |
|---|---|
| `list_customer` | Retrieve all Customer records |
| `get_customer_by_id` | Retrieve a single Customer by ID |
| `create_customer` | Create a new Customer (schema from `CustomerPostDto`) |
| `update_customer` | Update an existing Customer (schema from `CustomerPutDto`) |
| `delete_customer` | Delete a Customer by ID |

**Custom endpoint tools** are generated from any Minimal API endpoint annotated with `.WithSummary()` / `.WithDescription()`:

```C#
public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>
[
    // Standard CRUD → auto-discovered, no annotation needed
    app => app.MapGet<Customer, CustomerGetDto, int>(Route, Tags),
    app => app.MapPost<Customer, CustomerPostDto, CustomerGetDto>(Route, Tags),

    // Custom endpoints → annotate to expose as MCP tools
    app => app.MapGetRequest(Route + "/cached", Tags, async ([FromServices] ApiBase api) =>
            await api.Handle<Customer, List<CustomerGetDto>>(new CachedCustomerRequest()))
        .WithSummary("Get cached customers")
        .WithDescription("Returns a cached list of customers. Served from cache if available."),

    app => app.MapDeleteRequest<Customer, CustomerGetDto, int>(Route, Tags, id => new DeleteCustomerIntegrationRequest { Id = id })
        .WithSummary("Delete customer via integration flow")
        .WithDescription("Deletes a customer using the full integration delete workflow."),
];
```

#### Enrich tool schemas with DTO XML comments

Add `/// <summary>` comments to your DTO properties — the MCP server reads them automatically and includes them as `description` fields in the JSON schema. This tells the AI assistant exactly what each field means, including constraints and examples.

**Step 1:** Enable XML doc generation in your DTO project's `.csproj`:
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

**Step 2:** Add XML comments to your DTO properties:
```csharp
public class CustomerPostDto : IDto
{
    /// <summary>Full name of the customer, e.g. 'Acme Corp'. Maximum 100 characters.</summary>
    public string Name { get; set; }
}

public class CustomerPutDto : IDto
{
    /// <summary>Unique identifier of the customer to update.</summary>
    public int Id { get; set; }

    /// <summary>New full name of the customer, e.g. 'Acme Corp'. Maximum 100 characters.</summary>
    public string Name { get; set; }
}
```

The MCP tool schema for `create_customer` will then look like:
```json
{
  "name": "create_customer",
  "inputSchema": {
    "properties": {
      "name": {
        "type": "string",
        "description": "Full name of the customer, e.g. 'Acme Corp'. Maximum 100 characters."
      }
    }
  }
}
```

This enables AI assistants to ask the right questions when creating or updating records — e.g. _"What is the customer's full name?"_ — before sending the request.

---

**IOSP Command tools** are generated from your XML documentation — giving AI assistants insight into your business workflows:

```C#
/// <summary>Handles the deletion of a customer including all related data.</summary>
/// <remarks>Executes in parallel: retrieves customer, validates invoices, then deletes.</remarks>
public class DeleteCustomerIntegrationCommand(...) : IntegrationCommand<...> { }
```

> All tool calls go through the standard HTTP pipeline — your authentication and authorization middleware applies automatically.

#### Connect any MCP client

**Claude Desktop** (`claude_desktop_config.json`):
```json
{
  "mcpServers": {
    "my-api": {
      "url": "https://your-api.com/mcp"
    }
  }
}
```

**Cursor** (`.cursor/mcp.json`):
```json
{
  "mcpServers": {
    "my-api": {
      "url": "https://your-api.com/mcp"
    }
  }
}
```

Once connected, your AI assistant knows your full API surface and can answer _"what can you do with customers?"_ or execute _"create a customer named Acme Corp"_ directly.

---

### When using Controllers add this to your Program.cs
```C#
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // this is needed for "http patch" only. If you do not need to use patch, you can remove this line

// After Build()
app.MapControllers();
```

### When using GraphQL add this to your Program.cs
```C#
builder.Services.AddCleanCodeJN<MyDbContext>(options =>
{
    options.ApplicationAssemblies =
    [
        typeof(CleanCodeJN.GenericApis.Sample.Business.AssemblyRegistration).Assembly,
        typeof(CleanCodeJN.GenericApis.Sample.Core.AssemblyRegistration).Assembly,
        typeof(CleanCodeJN.GenericApis.Sample.Domain.AssemblyRegistration).Assembly
    ];
    options.ValidatorAssembly = typeof(CleanCodeJN.GenericApis.Sample.Core.AssemblyRegistration).Assembly;

    // Enable GraphQL with all CRUD operations
    options.GraphQLOptions = new GraphQLOptions
    {
        Get = true,
        Create = true,
        Update = true,
        Delete = true,
        AddAuthorizationWithPolicyName = "MyPolicy", // optional for adding authorization policy
    };
});

// Optional: Add Authentication and Authorization if needed
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MyPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("role", "admin");
    });
});
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

### Custom Middlewares

CleanCodeJN.GenericApis is fully compatible with the standard ASP.NET Core middleware pipeline.  
You can easily add **custom middlewares** for authentication, logging, exception handling, or any other cross-cutting concern — before or after the CleanCodeJN setup.

#### Where to Add Middlewares

Custom middlewares should be registered in your `Program.cs` **after** the `AddCleanCodeJN()` call, but **before** the CleanCodeJN 
middlewares such as `UseCleanCodeJNWith`. Global mediator behaviours for logging or caching can directly be added in the `AddCleanCodeJN()` options.

There already is a default logging behaviour included, which can be enabled in the options. This behaviour logs the execution and exection time of each command.
#### Example Structure

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add CleanCodeJN
builder.Services.AddCleanCodeJN<MyDbContext>(options =>
{
    options.AddDefaultLoggingBehavior = true; // Enables default logging behaviour
    options.OpenBehaviors = [typeof(CustomBehavior<,>)]; // Adds custom behaviour with 2 generic parameters for TRequest, TResponse

    options.ApplicationAssemblies = [typeof(Program).Assembly];
    options.ValidatorAssembly = typeof(Program).Assembly;
});

// Add custom services
builder.Services.AddLogging();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://your-keycloak-domain/auth/realms/yourrealm";
        options.Audience = "your-api";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Add your middlewares in the right order

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Custom Logging Middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("➡️ Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next();
    logger.LogInformation("⬅️ Response: {StatusCode}", context.Response.StatusCode);
});

// Global Exception Handling
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception occurred");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            Title = "Unexpected Error",
            Detail = ex.Message
        });
    }
});

// CleanCodeJN Middlewares
app.UseCleanCodeJNWithMinimalApis();
app.UseCleanCodeJNWithGraphQL();
app.UseCleanCodeJNWithMcp();
app.UseCleanCodeJNDocumentation();

// Run
app.Run();
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

### This is how clean your code will look like in the end
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

---

## 💬 AI Chat UI — `/ai` Page

The `CleanCodeJN.GenericApis.Chat` package provides a **ready-made Blazor WebAssembly chat page** at `/ai`. It connects directly to your backend's MCP server and lets users interact with your API through natural language — powered by Claude.

> Requires `UseCleanCodeJNWithMcp()` on the backend.

### Install

```bash
dotnet add package CleanCodeJN.GenericApis.Chat      # Blazor WASM UI component
```

### Backend — `Program.cs` additions

```csharp
// Register the AI proxy service (streams Claude responses + executes MCP tool calls)
builder.Services.AddHttpClient("AiProxy");
builder.Services.Configure<AiProxyOptions>(builder.Configuration.GetSection("AiProxy"));
builder.Services.AddScoped<AiProxyService>();
```

```json
// appsettings.json
{
  "AiProxy": {
    "AnthropicApiKey": "sk-ant-...",
    "Model": "claude-opus-4-5",
    "MaxTokens": 8096,
    "SelfBaseUrl": "https://localhost:7132"
  }
}
```

The backend must also expose the `/api/ai/stream` SSE endpoint — this is included automatically when `AiProxyService` is registered and the endpoint is mapped:

```csharp
app.MapPost("/api/ai/stream", async (
    ChatRequest request,
    AiProxyService aiProxy,
    HttpContext context) =>
{
    var bearer = context.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
    context.Response.ContentType = "text/event-stream";
    await foreach (var ev in aiProxy.StreamAsync(request, bearer, context.RequestAborted))
        await context.Response.WriteAsync($"data: {JsonSerializer.Serialize(ev)}\n\n");
});
```

### Blazor WASM — `Program.cs`

```csharp
using CleanCodeJN.GenericApis.Chat.Extensions;

builder.Services.AddMudServices();

builder.Services.AddCleanCodeJNWithAiChat(options =>
{
    options.BackendUrl = "https://localhost:7132"; // URL of your CleanCodeJN backend
    options.Title = "My AI Assistant";            // Title shown in the app bar
    options.ShowToolCalls = true;                 // Show tool call chips in the chat
    // options.BearerToken = "your-token";        // Optional: bearer token for auth
});
```

### `App.razor` — register the `/ai` route from the library

```razor
<Router AppAssembly="@typeof(App).Assembly"
        AdditionalAssemblies="@(new[] { typeof(CleanCodeJN.GenericApis.Chat.Pages.AiChatPage).Assembly })">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(Layout.MainLayout)" />
    </Found>
</Router>
```

### `Layout/MainLayout.razor` — MudBlazor dark theme

```razor
@inherits LayoutComponentBase

<MudThemeProvider IsDarkMode="true" Theme="_theme" />
<MudPopoverProvider />
<MudDialogProvider />
<MudSnackbarProvider />

@Body

@code {
    private readonly MudTheme _theme = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#4CAF50",
            AppbarBackground = "#0d1b2a",
            Background = "#0a1520",
            Surface = "#112233",
        }
    };
}
```

### What you get

| Feature | Description |
|---|---|
| **Streaming chat** | Claude responses stream token by token via SSE |
| **Tool sidebar** | All MCP tools listed — click to insert into input |
| **Tool call chips** | Visual indicator when Claude calls a tool and receives a result |
| **Markdown rendering** | Tables, code blocks, lists rendered with syntax highlighting |
| **Dark theme** | CleanCodeJN-branded dark UI out of the box |
| **Auto-redirect** | App opens directly at `/ai` on start |

---

# Sample Code
[GitHub Full Sample](https://github.com/decius999/CleanCodeJN-Generic-Apis/tree/dev/CleanCodeJN.GenericApis.Sample)
