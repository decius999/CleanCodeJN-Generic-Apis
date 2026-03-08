using CleanCodeJN.GenericApis.Extensions;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Extensions;

public class McpExtensionsTests
{
    // ─── ToSnakeCase ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Customer", "customer")]
    [InlineData("CustomerGetDto", "customer_get_dto")]
    [InlineData("MyLongEntityName", "my_long_entity_name")]
    [InlineData("invoice", "invoice")]
    [InlineData("A", "a")]
    public void ToSnakeCase_ShouldConvertCorrectly(string input, string expected)
        => Assert.Equal(expected, McpExtensions.ToSnakeCase(input));

    // ─── SnakeCaseToPascal ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("customer", "Customer")]
    [InlineData("customer_get_dto", "CustomerGetDto")]
    [InlineData("my_long_entity_name", "MyLongEntityName")]
    [InlineData("invoice", "Invoice")]
    public void SnakeCaseToPascal_ShouldConvertCorrectly(string input, string expected)
        => Assert.Equal(expected, McpExtensions.SnakeCaseToPascal(input));

    // ─── RouteToSnakeCase ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("api/v1/Customers", "api_v1_Customers")]
    [InlineData("api/v1/Customers/{id}", "api_v1_Customers_id")]
    [InlineData("api/v1/Customers/cached-by-request", "api_v1_Customers_cached-by-request")]
    [InlineData("api/v2/Invoices/{id}", "api_v2_Invoices_id")]
    public void RouteToSnakeCase_ShouldConvertCorrectly(string input, string expected)
        => Assert.Equal(expected, McpExtensions.RouteToSnakeCase(input));

    // ─── ResolveEntityNameFromToolName ──────────────────────────────────────────

    [Theory]
    [InlineData("list_customer", "Customer")]
    [InlineData("list_invoice", "Invoice")]
    [InlineData("get_customer_by_id", "Customer")]
    [InlineData("get_invoice_by_id", "Invoice")]
    [InlineData("create_customer", "Customer")]
    [InlineData("update_customer", "Customer")]
    [InlineData("delete_customer", "Customer")]
    public void ResolveEntityNameFromToolName_ShouldReturnEntityName(string toolName, string expected)
        => Assert.Equal(expected, McpExtensions.ResolveEntityNameFromToolName(toolName));

    [Theory]
    [InlineData("describe_cached_customer_command")]
    [InlineData("get_api_v1_Customers_cached")]  // no _by_id suffix → not matched
    [InlineData("unknown_tool")]
    public void ResolveEntityNameFromToolName_ShouldReturnNull_ForNonCrudTools(string toolName)
        => Assert.Null(McpExtensions.ResolveEntityNameFromToolName(toolName));

    // Custom-endpoint tools like delete_api_v1_Customers_id DO match "delete_" prefix.
    // The higher-level ExecuteToolCall checks custom endpoints first, so this never reaches entity lookup.
    [Fact]
    public void ResolveEntityNameFromToolName_DeleteWithRoutePath_ReturnsNonNullButIsRoutedBeforeEntityLookup()
        => Assert.NotNull(McpExtensions.ResolveEntityNameFromToolName("delete_api_v1_Customers_id"));

    // ─── GetJsonType ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(typeof(int), "integer")]
    [InlineData(typeof(long), "integer")]
    [InlineData(typeof(short), "integer")]
    [InlineData(typeof(byte), "integer")]
    [InlineData(typeof(int?), "integer")]
    [InlineData(typeof(double), "number")]
    [InlineData(typeof(float), "number")]
    [InlineData(typeof(decimal), "number")]
    [InlineData(typeof(bool), "boolean")]
    [InlineData(typeof(string), "string")]
    [InlineData(typeof(Guid), "string")]
    [InlineData(typeof(DateTime), "string")]
    [InlineData(typeof(List<string>), "array")]
    [InlineData(typeof(int[]), "array")]
    public void GetJsonType_ShouldMapCorrectly(Type type, string expected)
        => Assert.Equal(expected, McpExtensions.GetJsonType(type));
}
