using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Sample.Core.Dtos;
using CleanCodeJN.GenericApis.Sample.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.Sample.Controllers;

/// <summary>
/// Invoices controller providing CRUD endpoints for invoice entities.
/// </summary>
/// <param name="commandBus">IMediator instance.</param>
/// <param name="mapper">ICleanCodeMapper instance.</param>
[Tags("Invoices Controller based")]
[Route($"api/v2/[controller]")]
public class InvoicesController(IMediator commandBus, ICleanCodeMapper mapper)
    : ApiCrudControllerBase<Invoice, InvoiceGetDto, InvoicePostDto, InvoicePutDto, Guid>(commandBus, mapper)
{
    /// <summary>
    /// Gets the where clause used when retrieving all invoices, filtering to amounts greater than 10.
    /// </summary>
    public override Expression<Func<Invoice, bool>> GetWhere => x => x.Amount > 10.0m;

    /// <summary>
    /// Gets the navigation properties to include when retrieving a list of invoices.
    /// </summary>
    public override List<Expression<Func<Invoice, object>>> GetIncludes => [x => x.Customer];

    /// <summary>
    /// Gets the navigation properties to include when retrieving a single invoice by ID.
    /// </summary>
    public override List<Expression<Func<Invoice, object>>> GetByIdIncludes => [x => x.Customer];
}
