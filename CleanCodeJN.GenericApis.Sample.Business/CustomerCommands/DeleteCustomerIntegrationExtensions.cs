using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Sample.Core.Constants;
using CleanCodeJN.GenericApis.Sample.Domain;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

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
}
