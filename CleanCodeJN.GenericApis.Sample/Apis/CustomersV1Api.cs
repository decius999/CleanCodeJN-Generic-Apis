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
            where: x => x.Name.StartsWith("Customer"),
            includes: [x => x.Invoices],
            select: x => new Customer { Id = x.Id, Name = x.Name },
            ignoreQueryFilters: true),

        app => app.MapGetRequest(Route + "/cached", Tags, async ([FromServices] ApiBase api) =>
                await api.Handle<Customer, List<CustomerGetDto>>(new CachedCustomerRequest())),

        app => app.MapGetRequest<Customer, List<CustomerGetDto>>(Route + "/cached-by-request", Tags, () => new CachedCustomerRequest()),

        app => app.MapGetPaged<Customer, CustomerGetDto, int>(Route, Tags),

        app => app.MapGetFiltered<Customer, CustomerGetDto, int>(Route, Tags),

        app => app.MapGetById<Customer, CustomerGetDto, int>(Route, Tags),

        app => app.MapGetByIdRequest<Customer, CustomerGetDto, int>(Route + "/by-request", Tags, id => new GetByIdRequest<Customer, int> { Id = id }),

        app => app.MapPut<Customer, CustomerPutDto, CustomerGetDto>(Route, Tags),

        app => app.MapPutRequest<Customer, CustomerPutDto, CustomerGetDto>(Route+ "/by-request", Tags,
            dto => new PutRequest<Customer, CustomerPutDto>{ Dto = dto}),

        app => app.MapPost<Customer, CustomerPostDto, CustomerGetDto>(Route, Tags),

        app => app.MapPostRequest<Customer, CustomerPostDto, CustomerGetDto>(Route + "/by-request", Tags,
            dto => new PostRequest<Customer, CustomerPostDto> { Dto = dto } ),

        app => app.MapPatch<Customer, CustomerGetDto, int>(Route, Tags),

        app => app.MapPatchRequest<Customer, CustomerGetDto, int>(Route + "/by-request", Tags, (id, httpContext) =>
                    new PatchRequest<Customer, int>{ Id = id, HttpContext = httpContext }),

        app => app.MapDeleteRequest<Customer, CustomerGetDto, int>(Route, Tags, id => new DeleteCustomerIntegrationRequest { Id = id })
    ];
}
