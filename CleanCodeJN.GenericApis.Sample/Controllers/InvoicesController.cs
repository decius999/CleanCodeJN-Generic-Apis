using System.Linq.Expressions;
using AutoMapper;
using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Sample.Dtos;
using CleanCodeJN.GenericApis.Sample.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.Sample.Controllers;

[Tags("Invoices Controller based")]
[Route($"api/v3/[controller]")]

public class InvoicesController(IMediator commandBus, IMapper mapper)
    : ApiCrudControllerBase<Invoice, InvoiceGetDto, InvoicePostDto, InvoicePutDto, Guid>(commandBus, mapper)
{
    public override Expression<Func<Invoice, bool>> GetWhere => x => x.Amount > 10.0m;

    public override List<Expression<Func<Invoice, object>>> GetIncludes => [x => x.Customer];

    public override List<Expression<Func<Invoice, object>>> GetByIdIncludes => [x => x.Customer];
}

