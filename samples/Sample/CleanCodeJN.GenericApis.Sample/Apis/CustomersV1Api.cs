using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Contracts;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;
using CleanCodeJN.GenericApis.Sample.Core.Dtos;
using CleanCodeJN.GenericApis.Sample.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.Sample.Apis;

public class CustomersV1Api : IApi
{
    public List<string> Tags => ["Customers Minimal API"];

    public string Route => $"api/v1/Customers";

    public List<Func<WebApplication, RouteHandlerBuilder>> HttpMethods =>
    [
        app => app.MapGet<Customer, CustomerGetDto, int>(
            Route,
            Tags,
            includes: [x => x.Invoices],
            ignoreQueryFilters: true)
                .WithSummary("Get all customers including invoices")
                .WithDescription("Returns a list of all customers which starts with Customer in their Name including their invoices."),

        app => app.MapGetRequest(Route + "/cached", Tags, async ([FromServices] ApiBase api) =>
                await api.Handle<Customer, List<CustomerGetDto>>(new CachedCustomerRequest()))
            .WithSummary("Get cached customers")
            .WithDescription("Returns a cached list of all customers. The result is served from cache if available."),

        app => app.MapGetRequest<Customer, List<CustomerGetDto>>(Route + "/cached-by-request", Tags, () => new CachedCustomerRequest())
            .WithSummary("Get cached customers by request")
            .WithDescription("Returns a cached list of all customers using a dedicated CachedCustomerRequest handler."),

        app => app.MapGetPaged<Customer, CustomerGetDto, int>(Route, Tags),

        app => app.MapGetFiltered<Customer, CustomerGetDto, int>(Route, Tags),

        app => app.MapGetById<Customer, CustomerGetDto, int>(Route, Tags),

        app => app.MapGetByIdRequest<Customer, CustomerGetDto, int>(Route + "/by-request", Tags, id => new GetByIdRequest<Customer, int>
        {
            Id = id,
            Includes = [x => x.Invoices]
        })
            .WithSummary("Get customer by ID with invoices")
            .WithDescription("Returns a single customer by their ID. Includes the related invoices in the response."),

        app => app.MapPut<Customer, CustomerPutDto, CustomerGetDto>(Route, Tags),

        app => app.MapPutRequest<Customer, CustomerPutDto, CustomerGetDto>(Route + "/by-request", Tags,
            dto => new PutRequest<Customer, CustomerPutDto> { Dto = dto })
            .WithSummary("Update customer by request")
            .WithDescription("Updates an existing customer using a custom PUT request handler. Accepts a CustomerPutDto as the request body."),

        app => app.MapPost<Customer, CustomerPostDto, CustomerGetDto>(Route, Tags),

        app => app.MapPostRequest<Customer, CustomerPostDto, CustomerGetDto>(Route + "/by-request", Tags,
            dto => new PostRequest<Customer, CustomerPostDto> { Dto = dto })
            .WithSummary("Create customer by request")
            .WithDescription("Creates a new customer using a custom POST request handler. Accepts a CustomerPostDto as the request body."),

        app => app.MapPatch<Customer, CustomerGetDto, int>(Route, Tags),

        app => app.MapPatchRequest<Customer, CustomerGetDto, int>(Route + "/by-request", Tags,
            (id, httpContext) => new PatchRequest<Customer, int> { Id = id, HttpContext = httpContext })
            .WithSummary("Patch customer by request")
            .WithDescription("Partially updates a customer by ID using a custom PATCH request handler. Applies only the fields provided in the request body."),

        app => app.MapDeleteRequest<Customer, CustomerGetDto, int>(Route, Tags, id => new DeleteCustomerIntegrationRequest { Id = id })
            .WithSummary("Delete customer via integration request")
            .WithDescription("Deletes a customer by ID using the DeleteCustomerIntegrationRequest, which triggers the full integration delete flow.")
    ];
}
