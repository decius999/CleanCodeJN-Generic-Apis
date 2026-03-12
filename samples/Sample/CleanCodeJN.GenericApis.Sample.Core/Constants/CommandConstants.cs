namespace CleanCodeJN.GenericApis.Sample.Core.Constants;

/// <summary>
/// Contains string constants used to identify named command execution blocks.
/// </summary>
public static class CommandConstants
{
    /// <summary>
    /// Block name for the command that retrieves a customer by ID.
    /// </summary>
    public const string CustomerGetById = nameof(CustomerGetById);

    /// <summary>
    /// Block name for the command that retrieves the first invoice by ID.
    /// </summary>
    public const string InvoiceGetFirstById = nameof(InvoiceGetFirstById);

    /// <summary>
    /// Block name for the command that deletes a customer by ID.
    /// </summary>
    public const string DeleteCustomerById = nameof(DeleteCustomerById);
}
