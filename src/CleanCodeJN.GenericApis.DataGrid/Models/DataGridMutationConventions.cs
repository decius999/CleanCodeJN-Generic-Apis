namespace CleanCodeJN.GenericApis.DataGrid.Models;

/// <summary>
/// Configures the GraphQL mutation field name prefixes used by <see cref="Components.CCJNDataGrid{TDto,TPostDto,TPutDto}"/>.
/// Defaults match the server-side <c>CleanCodeNamingConventions</c> out of the box.
/// </summary>
public class DataGridMutationConventions
{
    /// <summary>Prefix for the create mutation. Default: <c>create</c> → e.g. <c>createCustomer</c>.</summary>
    public string CreatePrefix { get; set; } = "create";

    /// <summary>Prefix for the update mutation. Default: <c>update</c> → e.g. <c>updateCustomer</c>.</summary>
    public string UpdatePrefix { get; set; } = "update";

    /// <summary>Prefix for the delete mutation. Default: <c>delete</c> → e.g. <c>deleteCustomer</c>.</summary>
    public string DeletePrefix { get; set; } = "delete";
}
