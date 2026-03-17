namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Configuration options for the MCP server integration.
/// </summary>
public class McpOptions
{
    /// <summary>
    /// Optional delegate to exclude specific tools from the MCP tools list.
    /// Receives the tool name (format: <c>{httpMethod}_{route_in_snake_case}</c>,
    /// e.g. <c>delete_api_customers_{id}</c>) and returns <c>true</c> to exclude it.
    /// </summary>
    /// <example>
    /// Exclude all DELETE tools:
    /// <code>options.ExcludeTools = name => name.StartsWith("delete_");</code>
    /// </example>
    public Func<string, bool> ExcludeTools { get; set; }
}
