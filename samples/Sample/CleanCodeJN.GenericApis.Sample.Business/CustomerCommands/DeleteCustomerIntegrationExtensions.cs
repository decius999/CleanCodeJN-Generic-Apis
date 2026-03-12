using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Sample.Core.Constants;
using CleanCodeJN.GenericApis.Sample.Domain;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

/// <summary>
/// Provides extension methods for building the customer integration delete execution pipeline.
/// </summary>
public static class DeleteCustomerIntegrationExtensions
{
    /// <summary>
    /// Configures the execution context to retrieve a customer by their unique identifier.
    /// </summary>
    /// <remarks>This method adds a request to the execution context to fetch a customer by their ID,
    /// including related invoices.</remarks>
    /// <param name="executionContext">The current command execution context.</param>
    /// <param name="customerId">The unique identifier of the customer to retrieve.</param>
    /// <returns>An updated <see cref="ICommandExecutionContext"/> configured with the request to retrieve the specified
    /// customer.</returns>
    public static ICommandExecutionContext CustomerGetByIdRequest(this ICommandExecutionContext executionContext, int customerId) => executionContext
       .WithRequest(
           () => new GetByIdRequest<Customer, int>
           {
               Id = customerId,
               Includes = [x => x.Invoices],
           },
           CommandConstants.CustomerGetById);

    /// <summary>
    /// Creates a command execution context to retrieve the first invoice associated with a customer by its ID.
    /// </summary>
    /// <remarks>This method assumes that the customer and their invoices are already loaded in the execution
    /// context. If no invoices are associated with the customer, the request will not be executed.</remarks>
    /// <param name="executionContext">The current command execution context.</param>
    /// <returns>A new <see cref="ICommandExecutionContext"/> configured to execute a request for retrieving the first invoice of
    /// a customer.</returns>
    public static ICommandExecutionContext InvoiceGetFirstByIdRequest(this ICommandExecutionContext executionContext) => executionContext
      .WithRequest(
          () => new GetByIdRequest<Invoice, Guid>
          {
              Id = executionContext.Get<Customer>(CommandConstants.CustomerGetById).Invoices.First().Id,
          },
          checkBeforeExecution: () => executionContext.Get<Customer>(CommandConstants.CustomerGetById).Invoices?.Any() == true,
          blockName: CommandConstants.InvoiceGetFirstById);

    /// <summary>
    /// Creates and configures a delete request for removing a customer by their unique identifier.
    /// </summary>
    /// <remarks>This method retrieves the customer identifier from the execution context and creates a delete
    /// request for the specified customer. The delete operation is associated with the command constant <see
    /// cref="CommandConstants.DeleteCustomerById"/>.</remarks>
    /// <param name="executionContext">The command execution context used to retrieve the customer and execute the delete operation.</param>
    /// <returns>The updated <see cref="ICommandExecutionContext"/> configured with the delete request.</returns>
    public static ICommandExecutionContext DeleteCustomerByIdRequest(this ICommandExecutionContext executionContext) => executionContext
      .WithRequest(
          () => new DeleteRequest<Customer, int>
          {
              Id = executionContext.Get<Customer>(CommandConstants.CustomerGetById).Id,
          },
          CommandConstants.DeleteCustomerById);

    /// <summary>
    /// Configures the execution context to load customer and related invoice data in parallel.
    /// </summary>
    /// <remarks>This method adds parallel requests to the execution context for retrieving a customer by
    /// their ID  and an associated invoice. The invoice ID is determined by the logic provided in the request
    /// configuration.</remarks>
    /// <param name="executionContext">The execution context to configure.</param>
    /// <param name="customerId">The unique identifier of the customer to retrieve.</param>
    /// <returns>The updated <see cref="ICommandExecutionContext"/> configured with parallel requests to load the specified
    /// customer and a related invoice.</returns>
    public static ICommandExecutionContext LoadCustomersInParallelRequest(this ICommandExecutionContext executionContext, int customerId) => executionContext
      .WithParallelWhenAllRequests(
            [
                () => new GetByIdRequest<Customer, int>
                        {
                            Id = customerId,
                        },
                () => new GetByIdRequest<Invoice, Guid>
                        {
                            Id = Guid.NewGuid(), // This should be replaced with the actual logic to get the invoice ID related to the customer
                        },
            ], blockName: "Parallel Block");

    /// <summary>
    /// Configures the execution context to load an invoice by its identifier for the specified customer.
    /// </summary>
    /// <remarks>This method creates a request to retrieve an invoice by its unique identifier and associates
    /// it with the execution context. The invoice identifier is determined based on the parallel execution block and
    /// the specified index.</remarks>
    /// <param name="executionContext">The execution context in which the request is executed.</param>
    /// <returns>The updated execution context configured with the request to load the invoice.</returns>
    public static ICommandExecutionContext LoadInvoiceByIdRequest(this ICommandExecutionContext executionContext) => executionContext
        .WithRequest(
            () => new GetByIdRequest<Invoice, Guid>
            {
                Id = executionContext.GetParallelWhenAllByIndex<Invoice>("Parallel Block", 1).Id,
            });
}
