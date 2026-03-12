using CleanCodeJN.GenericApis.Extensions;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Extensions;

/// <summary>
/// Contains unit tests for string-conversion and JSON-type utility methods in <see cref="McpExtensions"/>.
/// </summary>
public class McpExtensionsTests
{
    // ─── ToSnakeCase ────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that PascalCase strings are correctly converted to snake_case.
    /// </summary>
    [Theory]
    [InlineData("Customer", "customer")]
    [InlineData("CustomerGetDto", "customer_get_dto")]
    [InlineData("MyLongEntityName", "my_long_entity_name")]
    [InlineData("invoice", "invoice")]
    [InlineData("A", "a")]
    public void ToSnakeCase_ShouldConvertCorrectly(string input, string expected)
        => Assert.Equal(expected, McpExtensions.ToSnakeCase(input));

    // ─── SnakeCaseToPascal ──────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that snake_case strings are correctly converted to PascalCase.
    /// </summary>
    [Theory]
    [InlineData("customer", "Customer")]
    [InlineData("customer_get_dto", "CustomerGetDto")]
    [InlineData("my_long_entity_name", "MyLongEntityName")]
    [InlineData("invoice", "Invoice")]
    public void SnakeCaseToPascal_ShouldConvertCorrectly(string input, string expected)
        => Assert.Equal(expected, McpExtensions.SnakeCaseToPascal(input));

    // ─── RouteToSnakeCase ───────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that API route strings are correctly normalised into snake_case tool-name format.
    /// </summary>
    [Theory]
    [InlineData("api/v1/Customers", "api_v1_customers")]
    [InlineData("api/v1/Customers/{id}", "api_v1_customers_id")]
    [InlineData("api/v1/Customers/cached-by-request", "api_v1_customers_cached-by-request")]
    [InlineData("api/v2/Invoices/{id}", "api_v2_invoices_id")]
    public void RouteToSnakeCase_ShouldConvertCorrectly(string input, string expected)
        => Assert.Equal(expected, McpExtensions.RouteToSnakeCase(input));

    // ─── GetJsonType ────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that .NET types are correctly mapped to their JSON Schema type names.
    /// </summary>
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
