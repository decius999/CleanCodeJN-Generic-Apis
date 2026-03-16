namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Configures the naming conventions used by CleanCodeJN to discover DTOs and generate GraphQL field names.
/// Override the defaults when your project uses different suffixes or prefixes.
/// </summary>
public class CleanCodeNamingConventions
{
    /// <summary>
    /// Suffix used to discover GET DTOs. Default: <c>GetDto</c>.
    /// Example: <c>CustomerGetDto</c>.
    /// </summary>
    public string GetDtoSuffix { get; set; } = "GetDto";

    /// <summary>
    /// Suffix used to discover POST (create) DTOs. Default: <c>PostDto</c>.
    /// Example: <c>CustomerPostDto</c>.
    /// </summary>
    public string PostDtoSuffix { get; set; } = "PostDto";

    /// <summary>
    /// Suffix used to discover PUT (update) DTOs. Default: <c>PutDto</c>.
    /// Example: <c>CustomerPutDto</c>.
    /// </summary>
    public string PutDtoSuffix { get; set; } = "PutDto";

    /// <summary>
    /// Prefix for the GraphQL create mutation field name. Default: <c>create</c>.
    /// Example: <c>createCustomer</c>.
    /// </summary>
    public string GraphQLCreatePrefix { get; set; } = "create";

    /// <summary>
    /// Prefix for the GraphQL update mutation field name. Default: <c>update</c>.
    /// Example: <c>updateCustomer</c>.
    /// </summary>
    public string GraphQLUpdatePrefix { get; set; } = "update";

    /// <summary>
    /// Prefix for the GraphQL delete mutation field name. Default: <c>delete</c>.
    /// Example: <c>deleteCustomer</c>.
    /// </summary>
    public string GraphQLDeletePrefix { get; set; } = "delete";
}
