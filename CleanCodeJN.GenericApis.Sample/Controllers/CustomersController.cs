using System.Linq.Expressions;
using AutoMapper;
using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Sample.Dtos;
using CleanCodeJN.GenericApis.Sample.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.Sample.Controllers;

[Tags("Customers Controller based")]
[Route($"api/v2/[controller]")]

public class CustomersController(IMediator commandBus, IMapper mapper)
    : ApiCrudControllerBase<Customer, CustomerGetDto, CustomerPostDto, CustomerPutDto, int>(commandBus, mapper)
{
    public override Expression<Func<Customer, bool>> GetWhere => x => x.Name.StartsWith("a");

    public override List<Expression<Func<Customer, object>>> GetIncludes => [x => x.Invoices];
}
