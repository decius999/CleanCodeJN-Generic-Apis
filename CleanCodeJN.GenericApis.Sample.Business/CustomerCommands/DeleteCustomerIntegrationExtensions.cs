using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Sample.Core.Constants;
using CleanCodeJN.GenericApis.Sample.Domain;

namespace CleanCodeJN.GenericApis.Sample.Business.CustomerCommands;

public static class DeleteCustomerIntegrationExtensions
{
    public static ICommandExecutionContext CustomerGetByIdRequest(this ICommandExecutionContext executionContext, int customerId) => executionContext
       .WithRequest(
           () => new GetByIdRequest<Customer, int>
           {
               Id = customerId,
               Includes = [x => x.Invoices],
           },
           CommandConstants.CustomerGetById);

    public static ICommandExecutionContext InvoiceGetFirstByIdRequest(this ICommandExecutionContext executionContext) => executionContext
      .WithRequest(
          () => new GetByIdRequest<Invoice, Guid>
          {
              Id = executionContext.Get<Customer>(CommandConstants.CustomerGetById).Invoices.First().Id,
          },
          checkBeforeExecution: () => executionContext.Get<Customer>(CommandConstants.CustomerGetById).Invoices?.Any() == true,
          blockName: CommandConstants.InvoiceGetFirstById);

    public static ICommandExecutionContext DeleteCustomerByIdRequest(this ICommandExecutionContext executionContext) => executionContext
      .WithRequest(
          () => new DeleteRequest<Customer, int>
          {
              Id = executionContext.Get<Customer>(CommandConstants.CustomerGetById).Id,
          },
          CommandConstants.DeleteCustomerById);
}
