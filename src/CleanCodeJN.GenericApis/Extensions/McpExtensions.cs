#nullable enable
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using CleanCodeJN.GenericApis.API;
using Microsoft.AspNetCore.Http.Metadata;
using Path = System.IO.Path;

namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Extension methods for Model Context Protocol (MCP) server integration.
/// Exposes API operations as MCP tools discoverable by AI assistants like Claude, Cursor, and others.
/// </summary>
public static class McpExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly Lazy<XDocument?> XmlDocs = new(() =>
    {
        try
        {
            return LoadMergedXmlDocs();
        }
        catch
        {
            return null;
        }
    });

    /// <summary>
    /// Maps a /mcp endpoint implementing the Model Context Protocol (MCP) Streamable HTTP transport.
    /// CRUD operations from ApiCrudControllerBase subclasses become executable MCP tools.
    /// IApi Minimal API registrations and IOSP Commands from XML docs become descriptive tools.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="configureOptions">Optional delegate to configure <see cref="McpOptions"/>, e.g. to exclude specific tools.</param>
    /// <returns>The web application.</returns>
    public static WebApplication UseCleanCodeJNWithMcp(this WebApplication app, Action<McpOptions>? configureOptions = null)
    {
        app.UseExceptionHandler();

        var mcpOptions = new McpOptions();
        configureOptions?.Invoke(mcpOptions);

        var callingAssembly = Assembly.GetCallingAssembly();

        app.MapPost("/mcp", async (HttpContext context) =>
        {
            context.Response.ContentType = "application/json";

            JsonObject request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<JsonObject>(context.Request.Body) ?? [];
            }
            catch
            {
                await WriteError(context, null, -32700, "Parse error");
                return;
            }

            var id = request["id"]?.DeepClone();
            var method = request["method"]?.GetValue<string>();

            if (method == null)
            {
                await WriteError(context, id, -32600, "Invalid Request: missing method");
                return;
            }

            if (method.StartsWith("notifications/"))
            {
                context.Response.StatusCode = 202;
                return;
            }

            try
            {
                var @params = request["params"] as JsonObject;

                var result = method switch
                {
                    "initialize" => BuildInitializeResult(),
                    "ping" => new { },
                    "tools/list" => BuildToolsListResult(callingAssembly, context.RequestServices, mcpOptions),
                    "tools/call" => await ExecuteToolCall(@params, context.RequestServices, callingAssembly, context),
                    _ => null
                };

                if (result == null)
                {
                    await WriteError(context, id, -32601, $"Method not found: {method}");
                    return;
                }

                var response = new JsonObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = id,
                    ["result"] = JsonSerializer.SerializeToNode(result, JsonOptions)
                };

                await context.Response.WriteAsync(response.ToJsonString());
            }
            catch (Exception ex)
            {
                await WriteError(context, id, -32603, ex.Message);
            }
        });

        return app;
    }

    // ─── MCP Protocol Handlers ──────────────────────────────────────────────────

    private static object BuildInitializeResult() => new
    {
        protocolVersion = "2024-11-05",
        serverInfo = new { name = "CleanCodeJN.GenericApis MCP Server", version = "1.0.0" },
        capabilities = new { tools = new { listChanged = false } }
    };

    private static object BuildToolsListResult(Assembly callingAssembly, IServiceProvider services, McpOptions mcpOptions)
    {
        var tools = new List<object>();
        var covered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Minimal API endpoints take priority: they carry WithSummary/WithDescription and outputSchema.
        // Controllers act as fallback for entities not covered by minimal API registrations.
        tools.AddRange(BuildMinimalApiCrudTools(services, covered));
        tools.AddRange(BuildControllerCrudTools(callingAssembly, covered, services));
        tools.AddRange(BuildWithSummaryTools(services, covered));
        tools.AddRange(BuildCommandDescriptionTools(covered));

        if (mcpOptions.ExcludeTools is not null)
        {
            tools.RemoveAll(t =>
            {
                var name = JsonSerializer.SerializeToNode(t, JsonOptions)?["name"]?.GetValue<string>();
                return name is not null && mcpOptions.ExcludeTools(name);
            });
        }

        return new { tools };
    }

    // ─── Tool Discovery ─────────────────────────────────────────────────────────

    private static IEnumerable<object> BuildControllerCrudTools(Assembly callingAssembly, HashSet<string> covered, IServiceProvider services)
    {
        var baseType = typeof(ApiCrudControllerBase<,,,,>);

        EndpointDataSource endpointDataSource;
        try
        {
            endpointDataSource = services.GetRequiredService<EndpointDataSource>();
        }
        catch
        {
            yield break;
        }

        foreach (var controllerType in callingAssembly.GetTypes().Where(t => !t.IsAbstract && IsSubclassOfGeneric(t, baseType)))
        {
            var genericBase = GetGenericBaseType(controllerType, baseType);
            if (genericBase == null)
            {
                continue;
            }

            var typeArgs = genericBase.GetGenericArguments();
            var entityType = typeArgs[0];
            var getDtoType = typeArgs[1];
            var postDtoType = typeArgs[2];
            var putDtoType = typeArgs[3];
            var keyType = typeArgs[4];

            var listSchema = new { type = "array", items = BuildDtoObjectSchema(getDtoType) };
            var singleSchema = BuildDtoObjectSchema(getDtoType);

            // Find controller endpoints by matching entity name in route (no CleanCodeEntityMetadata)
            var controllerEndpoints = endpointDataSource.Endpoints.OfType<RouteEndpoint>()
                .Where(e => e.Metadata.GetMetadata<CleanCodeEntityMetadata>() == null)
                .Where(e => (e.RoutePattern.RawText ?? "").Contains(entityType.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var ep in controllerEndpoints)
            {
                var httpMethod = ep.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault();
                if (httpMethod == null)
                {
                    continue;
                }

                var route = ep.RoutePattern.RawText ?? "";
                var toolName = $"{httpMethod.ToLowerInvariant()}_{RouteToSnakeCase(route)}";

                // Skip if a MinimalAPI tool with the same route-based name already exists
                if (covered.Contains(toolName))
                {
                    continue;
                }

                var hasId = ep.RoutePattern.Parameters.Any(p => p.Name.Equals("id", StringComparison.OrdinalIgnoreCase));

                switch (httpMethod.ToUpperInvariant())
                {
                    case "GET" when !hasId:
                        covered.Add(toolName);
                        yield return MakeTool(toolName, $"Retrieve all {entityType.Name} records.", BuildSchema(), listSchema);
                        break;
                    case "GET" when hasId:
                        covered.Add(toolName);
                        yield return MakeTool(toolName, $"Retrieve a single {entityType.Name} by its ID.",
                            BuildSchema(("id", GetJsonType(keyType), $"The {entityType.Name} identifier", true)), singleSchema);
                        break;
                    case "POST":
                        covered.Add(toolName);
                        yield return MakeTool(toolName, $"Create a new {entityType.Name}.", BuildDtoSchema(postDtoType), singleSchema);
                        break;
                    case "PUT":
                        covered.Add(toolName);
                        yield return MakeTool(toolName, $"Update an existing {entityType.Name}.", BuildDtoSchema(putDtoType), singleSchema);
                        break;
                    case "DELETE":
                        covered.Add(toolName);
                        yield return MakeTool(toolName, $"Delete a {entityType.Name} by its ID.",
                            BuildSchema(("id", GetJsonType(keyType), $"The {entityType.Name} identifier to delete", true)), singleSchema);
                        break;
                }
            }
        }
    }

    private static IEnumerable<object> BuildMinimalApiCrudTools(IServiceProvider services, HashSet<string> covered)
    {
        EndpointDataSource endpointDataSource;
        try
        {
            endpointDataSource = services.GetRequiredService<EndpointDataSource>();
        }
        catch
        {
            yield break;
        }

        foreach (var (endpoint, meta) in endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Select(e => (Endpoint: e, Meta: e.Metadata.GetMetadata<CleanCodeEntityMetadata>()))
            .Where(x => x.Meta != null))
        {
            var entityName = meta!.EntityType.Name;
            var httpMethod = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "GET";
            var route = endpoint.RoutePattern.RawText ?? "";
            var toolName = $"{httpMethod.ToLowerInvariant()}_{RouteToSnakeCase(route)}";

            if (covered.Contains(toolName))
                continue;

            var getDtoType = meta.GetDtoType;
            var listSchema = new { type = "array", items = BuildDtoObjectSchema(getDtoType) };
            var singleSchema = BuildDtoObjectSchema(getDtoType);
            var description = GetEndpointDescription(endpoint);

            switch (meta.Operation)
            {
                case "LIST":
                    covered.Add(toolName);
                    yield return MakeTool(toolName,
                        description ?? $"Retrieve all {entityName} records.",
                        BuildSchema(), listSchema);
                    break;

                case "LIST_PAGED":
                    covered.Add(toolName);
                    yield return MakeTool(toolName,
                        description ?? $"Retrieve a paged list of {entityName} records.",
                        BuildSchema(
                            ("page", "integer", "Page number (1-based).", true),
                            ("pageSize", "integer", "Number of records per page.", true),
                            ("direction", "string", "Sort direction: 'asc' or 'desc'.", true),
                            ("sortBy", "string", $"Property name of {entityName} to sort by.", true)),
                        listSchema);
                    break;

                case "LIST_FILTERED":
                    covered.Add(toolName);
                    yield return MakeTool(toolName,
                        description ?? $"Retrieve a filtered and paged list of {entityName} records.",
                        BuildSchema(
                            ("page", "integer", "Page number (1-based).", true),
                            ("pageSize", "integer", "Number of records per page.", true),
                            ("direction", "string", "Sort direction: 'asc' or 'desc'.", true),
                            ("sortBy", "string", $"Property name of {entityName} to sort by.", true),
                            ("filter", "string", "Filter string applied server-side.", true)),
                        listSchema);
                    break;

                case "GET_BY_ID" when meta.KeyType != null:
                    covered.Add(toolName);
                    yield return MakeTool(toolName,
                        description ?? $"Retrieve a single {entityName} by its ID.",
                        BuildSchema(("id", GetJsonType(meta.KeyType), $"The {entityName} identifier", true)), singleSchema);
                    break;

                case "POST" when meta.WriteType != null:
                    covered.Add(toolName);
                    yield return MakeTool(toolName,
                        description ?? $"Create a new {entityName}.",
                        BuildDtoSchema(meta.WriteType), singleSchema);
                    break;

                case "PUT" when meta.WriteType != null:
                    covered.Add(toolName);
                    yield return MakeTool(toolName,
                        description ?? $"Update an existing {entityName}.",
                        BuildDtoSchema(meta.WriteType), singleSchema);
                    break;

                case "PATCH" when meta.KeyType != null:
                    covered.Add(toolName);
                    yield return MakeTool(toolName,
                        description ?? $"Partially update an existing {entityName}. Provide only the fields you want to change.",
                        BuildPatchSchema(meta.KeyType, getDtoType), singleSchema);
                    break;

                case "DELETE" when meta.KeyType != null:
                    covered.Add(toolName);
                    yield return MakeTool(toolName,
                        description ?? $"Delete a {entityName} by its ID.",
                        BuildSchema(("id", GetJsonType(meta.KeyType), $"The {entityName} identifier to delete", true)), singleSchema);
                    break;
            }
        }
    }

    /// <summary>
    /// Returns the WithSummary + WithDescription text of an endpoint, or null if neither is set.
    /// Combined as "Summary — Description" when both are present.
    /// </summary>
    private static string? GetEndpointDescription(RouteEndpoint endpoint)
    {
        var summary = endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>()?.Summary;
        var description = endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description;
        var parts = new[] { summary, description }.Where(s => !string.IsNullOrWhiteSpace(s));
        var combined = string.Join(" — ", parts);
        return string.IsNullOrWhiteSpace(combined) ? null : combined;
    }

    private static IEnumerable<object> BuildWithSummaryTools(IServiceProvider services, HashSet<string> covered)
    {
        EndpointDataSource endpointDataSource;
        try
        {
            endpointDataSource = services.GetRequiredService<EndpointDataSource>();
        }
        catch
        {
            yield break;
        }

        foreach (var endpoint in endpointDataSource.Endpoints.OfType<RouteEndpoint>())
        {
            // Skip CRUD endpoints already handled via CleanCodeEntityMetadata
            if (endpoint.Metadata.GetMetadata<CleanCodeEntityMetadata>() != null)
            {
                continue;
            }

            var summary = endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>()?.Summary;
            var description = endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description;
            if (string.IsNullOrWhiteSpace(summary) && string.IsNullOrWhiteSpace(description))
            {
                continue;
            }

            var httpMethod = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "GET";
            var route = endpoint.RoutePattern.RawText ?? "";
            var endpointName = endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
            var toolName = endpointName != null
                ? ToSnakeCase(endpointName)
                : $"{httpMethod.ToLowerInvariant()}_{RouteToSnakeCase(route)}";

            if (covered.Contains(toolName))
                continue;

            covered.Add(toolName);

            var fullDesc = string.Join(" — ", new[] { summary, description }.Where(s => !string.IsNullOrWhiteSpace(s)));

            var routeParams = endpoint.RoutePattern.Parameters
                .Select(p => (p.Name, "string", $"Route parameter {p.Name}", true))
                .ToArray();

            yield return MakeTool(toolName, fullDesc, BuildSchema(routeParams));
        }
    }

    private static IEnumerable<object> BuildCommandDescriptionTools(HashSet<string> covered)
    {
        XDocument xml;
        try
        {
            xml = LoadMergedXmlDocs();
        }
        catch
        {
            yield break;
        }

        foreach (var member in xml.Descendants("member")
            .Where(x => x.Attribute("name")?.Value.StartsWith("T:") == true &&
                        x.Attribute("name")!.Value.EndsWith("Command")))
        {
            var fullName = member.Attribute("name")!.Value[2..];
            var name = fullName.Split('.').Last();
            var toolName = $"describe_{ToSnakeCase(name)}";

            if (covered.Contains(toolName))
                continue;

            covered.Add(toolName);

            var summary = GetXmlText(member.Element("summary"));
            var remarks = GetXmlText(member.Element("remarks"));
            var xmlParams = member.Elements("param")
                .Select(p => ("param_" + (p.Attribute("name")?.Value ?? "value"), "string", GetXmlText(p), false))
                .ToArray();

            var desc = new StringBuilder(string.IsNullOrWhiteSpace(summary) ? $"The {name} IOSP command." : summary);
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                desc.Append($" Remarks: {remarks}");
            }

            desc.Append(" Call /api/docs for full workflow documentation.");

            yield return MakeTool(toolName, desc.ToString(), BuildSchema(xmlParams));
        }
    }

    // ─── Tool Execution ─────────────────────────────────────────────────────────

    private static async Task<object> ExecuteToolCall(JsonObject? @params, IServiceProvider services, Assembly callingAssembly, HttpContext httpContext)
    {
        var toolName = @params?["name"]?.GetValue<string>() ?? "";
        var args = @params?["arguments"] as JsonObject ?? [];

        if (toolName.StartsWith("describe_"))
        {
            return SuccessContent("This is a descriptive tool. Visit /docs or /api/docs for full API documentation.");
        }

        // Find endpoint by matching its route-based tool name (works for both CRUD and custom endpoints)
        var endpointDs = services.GetRequiredService<EndpointDataSource>();
        var endpoint = endpointDs.Endpoints.OfType<RouteEndpoint>()
            .FirstOrDefault(e =>
            {
                var endpointName = e.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
                var method = e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "GET";
                var route = e.RoutePattern.RawText ?? "";
                var expected = endpointName != null ? ToSnakeCase(endpointName) : $"{method.ToLowerInvariant()}_{RouteToSnakeCase(route)}";
                return expected == toolName;
            });

        return endpoint == null ? ErrorContent($"Unknown tool: {toolName}") : await ExecuteEndpointViaHttp(args, endpoint, httpContext);
    }

    private static async Task<object> ExecuteEndpointViaHttp(JsonObject args, RouteEndpoint endpoint, HttpContext httpContext)
    {
        var httpMethod = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "GET";
        var routeParamNames = endpoint.RoutePattern.Parameters.Select(p => p.Name).ToHashSet();

        // Fill route parameters from args
        var url = endpoint.RoutePattern.RawText ?? "";
        foreach (var param in endpoint.RoutePattern.Parameters)
        {
            url = url.Replace($"{{{param.Name}}}", args[param.Name]?.ToString() ?? "");
        }

        // Remaining args → query string for GET/DELETE, body for POST/PUT/PATCH
        var nonRouteArgs = args.Where(kv => !routeParamNames.Contains(kv.Key)).ToList();
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var fullUrl = $"{baseUrl}/{url.TrimStart('/')}";

        if (httpMethod is "GET" or "DELETE" && nonRouteArgs.Count > 0)
        {
            var qs = string.Join("&", nonRouteArgs.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value?.ToString() ?? "")}"));
            fullUrl += "?" + qs;
        }

        using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
        using var client = new HttpClient(handler);
        ForwardAuthHeader(httpContext, client);

        HttpRequestMessage req;
        if (httpMethod is "GET" or "DELETE")
        {
            req = new HttpRequestMessage(new HttpMethod(httpMethod), fullUrl);
        }
        else if (httpMethod is "PATCH")
        {
            // JSON Patch (RFC 6902): convert flat args into replace-operations array
            var ops = new JsonArray();
            foreach (var kv in nonRouteArgs)
            {
                ops.Add(new JsonObject
                {
                    ["op"] = "replace",
                    ["path"] = "/" + kv.Key,
                    ["value"] = kv.Value?.DeepClone()
                });
            }

            req = new HttpRequestMessage(new HttpMethod(httpMethod), fullUrl)
            {
                Content = new StringContent(ops.ToJsonString(), Encoding.UTF8, "application/json-patch+json")
            };
        }
        else
        {
            var body = new JsonObject();
            foreach (var kv in nonRouteArgs)
            {
                body[kv.Key] = kv.Value?.DeepClone();
            }

            req = new HttpRequestMessage(new HttpMethod(httpMethod), fullUrl)
            {
                Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
            };
        }

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            return ErrorContent($"HTTP call failed ({httpMethod} {fullUrl}): {ex.Message}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return ErrorContent($"HTTP {(int)response.StatusCode} ({httpMethod} {fullUrl}): {responseBody}");

        return SuccessContent(responseBody);
    }

    private static void ForwardAuthHeader(HttpContext httpContext, HttpClient client)
    {
        if (httpContext.Request.Headers.TryGetValue("Authorization", out var auth))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", (string?)auth);
        }
    }

    /// <summary>
    /// Converts an ASP.NET Core route pattern string (e.g. "api/customer/{id}") to a snake_case tool name.
    /// </summary>
    /// <param name="route">The route pattern string to convert.</param>
    /// <returns>A snake_case string suitable for use as an MCP tool name.</returns>
    internal static string RouteToSnakeCase(string route) =>
        string.Join("_", route.Replace("{", "").Replace("}", "")
            .Split('/', StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();

    // ─── Schema Builders ────────────────────────────────────────────────────────

    private static object MakeTool(string name, string description, object inputSchema, object? outputSchema = null) =>
        outputSchema != null
            ? (new { name, description, inputSchema, outputSchema })
            : new { name, description, inputSchema };

    /// <summary>
    /// Builds a JSON schema object for a DTO type, with cycle detection to handle
    /// circular navigation properties (e.g. Customer → Invoice → Customer).
    /// </summary>
    private static object BuildDtoObjectSchema(Type type, HashSet<Type>? visited = null)
    {
        visited ??= [];
        if (!visited.Add(type))
        {
            return new { type = "object" }; // circular reference — stop expanding
        }

        var properties = new Dictionary<string, object>();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead))
        {
            properties[JsonNamingPolicy.CamelCase.ConvertName(prop.Name)] = BuildPropertySchema(prop.PropertyType, visited);
        }

        return new { type = "object", properties };
    }

    private static object BuildPropertySchema(Type type, HashSet<Type> visited)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;

        if (t == typeof(string) || t == typeof(Guid))
        {
            return new { type = "string" };
        }

        if (t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte))
        {
            return new { type = "integer" };
        }

        if (t == typeof(double) || t == typeof(float) || t == typeof(decimal))
        {
            return new { type = "number" };
        }

        if (t == typeof(bool))
        {
            return new { type = "boolean" };
        }

        if (t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(DateOnly))
        {
            return new { type = "string" };
        }

        if (t != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(t))
        {
            var itemType = t.IsArray ? t.GetElementType() : t.GetGenericArguments().FirstOrDefault();
            return itemType != null
                ? (new { type = "array", items = BuildPropertySchema(itemType, visited) })
                : new { type = "array" };
        }

        return t.IsClass ? BuildDtoObjectSchema(t, visited) : (new { type = "string" });
    }

    private static object BuildSchema(params (string name, string type, string description, bool required)[] props)
    {
        var properties = new Dictionary<string, object>();
        var required = new List<string>();
        foreach (var (n, t, d, r) in props)
        {
            properties[n] = new { type = t, description = d };
            if (r)
            {
                required.Add(n);
            }
        }

        return required.Count > 0
            ? (new { type = "object", properties, required })
            : new { type = "object", properties };
    }

    private static object BuildDtoSchema(Type dtoType)
    {
        var properties = new Dictionary<string, object>();
        var required = new List<string>();
        var visited = new HashSet<Type>();

        var nullabilityCtx = new NullabilityInfoContext();
        foreach (var prop in dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite))
        {
            var camelName = JsonNamingPolicy.CamelCase.ConvertName(prop.Name);
            var description = GetXmlPropertyDescription(dtoType, prop) ?? $"{prop.Name} property.";

            var underlying = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            var isPrimitive = underlying == typeof(string) || underlying == typeof(Guid)
                || underlying == typeof(int) || underlying == typeof(long) || underlying == typeof(short) || underlying == typeof(byte)
                || underlying == typeof(double) || underlying == typeof(float) || underlying == typeof(decimal)
                || underlying == typeof(bool) || underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset) || underlying == typeof(DateOnly);

            if (isPrimitive)
            {
                properties[camelName] = new { type = GetJsonType(prop.PropertyType), description };
            }
            else
            {
                // Complex type or collection: build full nested schema
                properties[camelName] = BuildPropertySchema(prop.PropertyType, visited);
            }

            // Required: non-nullable value types, and reference types that are not explicitly nullable (string? etc.)
            var isExplicitlyNullable = Nullable.GetUnderlyingType(prop.PropertyType) != null
                || nullabilityCtx.Create(prop).WriteState == NullabilityState.Nullable;
            if (!isExplicitlyNullable)
            {
                required.Add(camelName);
            }
        }

        return required.Count > 0
            ? (new { type = "object", properties, required })
            : new { type = "object", properties };
    }

    /// <summary>
    /// Builds the input schema for a PATCH tool: id is required, all other DTO fields are optional.
    /// The AI provides only the fields it wants to update; the executor converts them to JSON Patch replace-ops.
    /// </summary>
    private static object BuildPatchSchema(Type keyType, Type getDtoType)
    {
        var properties = new Dictionary<string, object>
        {
            ["id"] = new { type = GetJsonType(keyType), description = "The identifier of the entity to patch." }
        };

        var visited = new HashSet<Type>();
        foreach (var prop in getDtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead))
        {
            var camelName = JsonNamingPolicy.CamelCase.ConvertName(prop.Name);
            if (camelName == "id")
                continue;

            properties[camelName] = BuildPropertySchema(prop.PropertyType, visited);
        }

        return new { type = "object", properties, required = new[] { "id" } };
    }

    /// <summary>
    /// Returns the JSON Schema type string ("string", "integer", "number", "boolean", or "array") for a .NET type.
    /// </summary>
    /// <param name="type">The .NET type to map.</param>
    /// <returns>A JSON Schema type string representing the .NET type.</returns>
    internal static string GetJsonType(Type type)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        if (t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte))
        {
            return "integer";
        }

        if (t == typeof(double) || t == typeof(float) || t == typeof(decimal))
        {
            return "number";
        }

        if (t == typeof(bool))
        {
            return "boolean";
        }

        return t.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(t) ? "array" : t.IsArray ? "array" : "string";
    }

    // ─── Reflection Helpers ─────────────────────────────────────────────────────

    private static bool IsSubclassOfGeneric(Type type, Type generic)
    {
        while (type != null && type != typeof(object))
        {
            var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (generic == cur)
            {
                return true;
            }

            type = type.BaseType!;
        }

        return false;
    }

    private static Type? GetGenericBaseType(Type type, Type generic)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == generic)
            {
                return type;
            }

            type = type.BaseType!;
        }

        return null;
    }

    private static void SetProp(object obj, string propName, object? value) =>
        obj.GetType().GetProperty(propName)?.SetValue(obj, value);

    /// <summary>
    /// Converts a PascalCase or camelCase name to snake_case.
    /// </summary>
    /// <param name="name">The name to convert.</param>
    /// <returns>The snake_case representation of the name.</returns>
    internal static string ToSnakeCase(string name)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0)
            {
                sb.Append('_');
            }

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a snake_case name to PascalCase.
    /// </summary>
    /// <param name="name">The snake_case name to convert.</param>
    /// <returns>The PascalCase representation of the name.</returns>
    internal static string SnakeCaseToPascal(string name) =>
        string.Concat(name.Split('_').Select(s => s.Length > 0 ? char.ToUpperInvariant(s[0]) + s[1..] : s));

    // ─── Response Helpers ───────────────────────────────────────────────────────

    private static object SuccessContent(string text) =>
        new { content = new[] { new { type = "text", text } } };

    private static object ErrorContent(string message) =>
        new { content = new[] { new { type = "text", text = $"Error: {message}" } }, isError = true };

    private static async Task WriteError(HttpContext context, JsonNode? id, int code, string message)
    {
        var response = new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["error"] = new JsonObject { ["code"] = code, ["message"] = message }
        };
        await context.Response.WriteAsync(response.ToJsonString());
    }

    // ─── XML Documentation Helpers ──────────────────────────────────────────────

    private static XDocument LoadMergedXmlDocs()
    {
        var baseDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? AppContext.BaseDirectory)!;
        var xmlFiles = Directory.GetFiles(baseDir, "*.xml", SearchOption.TopDirectoryOnly);
        var merged = new XDocument(new XElement("doc",
            new XElement("assembly", new XElement("name", "MergedDocs")),
            new XElement("members")));
        var membersRoot = merged.Root!.Element("members")!;

        foreach (var file in xmlFiles)
        {
            try
            {
                var doc = XDocument.Load(file);
                foreach (var m in doc.Descendants("member"))
                {
                    if (!membersRoot.Elements("member").Any(e => (string)e.Attribute("name")! == (string)m.Attribute("name")!))
                    {
                        membersRoot.Add(new XElement(m));
                    }
                }
            }
            catch { }
        }

        return merged;
    }

    private static string? GetXmlPropertyDescription(Type dtoType, PropertyInfo prop)
    {
        var xml = XmlDocs.Value;
        if (xml == null)
        {
            return null;
        }

        var memberName = $"P:{dtoType.FullName}.{prop.Name}";
        var member = xml.Descendants("member")
            .FirstOrDefault(x => x.Attribute("name")?.Value == memberName);
        var text = member == null ? null : GetXmlText(member.Element("summary"));
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static string GetXmlText(XElement? element)
    {
        if (element == null)
        {
            return string.Empty;
        }

        var text = element.Nodes()
            .Select(node =>
            {
                if (node is XElement el && el.Name.LocalName == "see")
                {
                    var cref = el.Attribute("cref")?.Value ?? "";
                    return cref.StartsWith("T:") || cref.StartsWith("M:") ? cref[2..] : cref;
                }

                return node.ToString(SaveOptions.DisableFormatting);
            })
            .Aggregate(string.Empty, (a, b) => a + b);

        return System.Text.RegularExpressions.Regex.Replace(text, "<.*?>", string.Empty)
            .Replace("\r", " ").Replace("\n", " ").Trim();
    }
}
