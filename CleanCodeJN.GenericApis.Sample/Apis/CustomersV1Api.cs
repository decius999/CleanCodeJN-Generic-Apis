﻿using CleanCodeJN.GenericApis.Contracts;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.GenericApis.Sample.Dtos;
using CleanCodeJN.GenericApis.Sample.Models;

namespace CleanCodeJN.GenericApis.Sample.Apis;

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
        app => app.MapDelete<Customer, CustomerGetDto>(Route, Tags)
    ];
}
