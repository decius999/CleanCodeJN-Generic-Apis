using System.Linq.Expressions;
using AutoMapper;
using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Sample.Core.Dtos;
using CleanCodeJN.GenericApis.Sample.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.Sample.Controllers;

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
