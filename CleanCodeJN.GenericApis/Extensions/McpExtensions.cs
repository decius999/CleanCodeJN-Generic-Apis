#nullable enable
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using CleanCodeJN.GenericApis.API;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Mvc;
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
        try { return LoadMergedXmlDocs(); }
        catch { return null; }
    });

    /// <summary>
    /// Maps a /mcp endpoint implementing the Model Context Protocol (MCP) Streamable HTTP transport.
    /// CRUD operations from ApiCrudControllerBase subclasses become executable MCP tools.
    /// IApi Minimal API registrations and IOSP Commands from XML docs become descriptive tools.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application.</returns>
    public static WebApplication UseCleanCodeJNWithMcp(this WebApplication app)
    {
        var callingAssembly = Assembly.GetCallingAssembly();

        app.MapPost("/mcp", async (HttpContext context) =>
        {
            context.Response.ContentType = "application/json";

            JsonObject request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<JsonObject>(context.Request.Body) ?? new JsonObject();
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

                object? result = method switch
                {
                    "initialize" => BuildInitializeResult(),
                    "ping" => new { },
                    "tools/list" => BuildToolsListResult(callingAssembly, context.RequestServices),
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

    private static object BuildToolsListResult(Assembly callingAssembly, IServiceProvider services)
    {
        var tools = new List<object>();
        var covered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        tools.AddRange(BuildControllerCrudTools(callingAssembly, covered));
        tools.AddRange(BuildMinimalApiCrudTools(services, covered));
        tools.AddRange(BuildWithSummaryTools(services));
        tools.AddRange(BuildCommandDescriptionTools());
        return new { tools };
    }

    // ─── Tool Discovery ─────────────────────────────────────────────────────────

    private static IEnumerable<object> BuildControllerCrudTools(Assembly callingAssembly, HashSet<string> covered)
    {
        var baseType = typeof(ApiCrudControllerBase<,,,,>);

        foreach (var controllerType in callingAssembly.GetTypes().Where(t => !t.IsAbstract && IsSubclassOfGeneric(t, baseType)))
        {
            var genericBase = GetGenericBaseType(controllerType, baseType);
            if (genericBase == null) continue;

            var args = genericBase.GetGenericArguments();
            var entityType = args[0];
            var postDtoType = args[2];
            var putDtoType = args[3];
            var keyType = args[4];

            var prefix = ToSnakeCase(entityType.Name);
            covered.Add(entityType.Name);

            yield return MakeTool($"list_{prefix}", $"Retrieve all {entityType.Name} records.", BuildSchema());
            yield return MakeTool($"get_{prefix}_by_id", $"Retrieve a single {entityType.Name} by its ID.",
                BuildSchema(("id", GetJsonType(keyType), $"The {entityType.Name} identifier", true)));
            yield return MakeTool($"create_{prefix}", $"Create a new {entityType.Name}.", BuildDtoSchema(postDtoType));
            yield return MakeTool($"update_{prefix}", $"Update an existing {entityType.Name}.", BuildDtoSchema(putDtoType));
            yield return MakeTool($"delete_{prefix}", $"Delete a {entityType.Name} by its ID.",
                BuildSchema(("id", GetJsonType(keyType), $"The {entityType.Name} identifier to delete", true)));
        }
    }

    private static IEnumerable<object> BuildMinimalApiCrudTools(IServiceProvider services, HashSet<string> covered)
    {
        EndpointDataSource endpointDataSource;
        try { endpointDataSource = services.GetRequiredService<EndpointDataSource>(); }
        catch { yield break; }

        // Group by entity type, collect one metadata per operation
        var entityGroups = endpointDataSource.Endpoints
            .Select(e => e.Metadata.GetMetadata<API.CleanCodeEntityMetadata>())
            .Where(m => m != null)
            .GroupBy(m => m!.EntityType.Name)
            .Where(g => !covered.Contains(g.Key));

        foreach (var group in entityGroups)
        {
            var ops = group.ToDictionary(m => m!.Operation, m => m!);
            var entityName = group.Key;
            var prefix = ToSnakeCase(entityName);

            if (ops.TryGetValue("LIST", out var listMeta) || ops.TryGetValue("LIST_PAGED", out listMeta))
                yield return MakeTool($"list_{prefix}", $"Retrieve all {entityName} records.", BuildSchema());

            if (ops.TryGetValue("GET_BY_ID", out var byIdMeta) && byIdMeta.KeyType != null)
                yield return MakeTool($"get_{prefix}_by_id", $"Retrieve a single {entityName} by its ID.",
                    BuildSchema(("id", GetJsonType(byIdMeta.KeyType), $"The {entityName} identifier", true)));

            if (ops.TryGetValue("POST", out var postMeta) && postMeta.WriteType != null)
                yield return MakeTool($"create_{prefix}", $"Create a new {entityName}.", BuildDtoSchema(postMeta.WriteType));

            if (ops.TryGetValue("PUT", out var putMeta) && putMeta.WriteType != null)
                yield return MakeTool($"update_{prefix}", $"Update an existing {entityName}.", BuildDtoSchema(putMeta.WriteType));

            if (ops.TryGetValue("DELETE", out var deleteMeta) && deleteMeta.KeyType != null)
                yield return MakeTool($"delete_{prefix}", $"Delete a {entityName} by its ID.",
                    BuildSchema(("id", GetJsonType(deleteMeta.KeyType), $"The {entityName} identifier to delete", true)));
        }
    }

    private static IEnumerable<object> BuildWithSummaryTools(IServiceProvider services)
    {
        EndpointDataSource endpointDataSource;
        try { endpointDataSource = services.GetRequiredService<EndpointDataSource>(); }
        catch { yield break; }

        foreach (var endpoint in endpointDataSource.Endpoints.OfType<RouteEndpoint>())
        {
            // Skip CRUD endpoints already handled via CleanCodeEntityMetadata
            if (endpoint.Metadata.GetMetadata<CleanCodeEntityMetadata>() != null) continue;

            var summary = endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>()?.Summary;
            var description = endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description;
            if (string.IsNullOrWhiteSpace(summary) && string.IsNullOrWhiteSpace(description)) continue;

            var httpMethod = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "GET";
            var route = endpoint.RoutePattern.RawText ?? "";
            var endpointName = endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
            var toolName = endpointName != null
                ? ToSnakeCase(endpointName)
                : $"{httpMethod.ToLowerInvariant()}_{RouteToSnakeCase(route)}";

            var fullDesc = string.Join(" — ", new[] { summary, description }.Where(s => !string.IsNullOrWhiteSpace(s)));

            var routeParams = endpoint.RoutePattern.Parameters
                .Select(p => (p.Name, "string", $"Route parameter {p.Name}", true))
                .ToArray();

            yield return MakeTool(toolName, fullDesc, BuildSchema(routeParams));
        }
    }

    private static IEnumerable<object> BuildCommandDescriptionTools()
    {
        XDocument xml;
        try { xml = LoadMergedXmlDocs(); }
        catch { yield break; }

        foreach (var member in xml.Descendants("member")
            .Where(x => x.Attribute("name")?.Value.StartsWith("T:") == true &&
                        x.Attribute("name")!.Value.EndsWith("Command")))
        {
            var fullName = member.Attribute("name")!.Value[2..];
            var name = fullName.Split('.').Last();
            var summary = GetXmlText(member.Element("summary"));
            var remarks = GetXmlText(member.Element("remarks"));
            var xmlParams = member.Elements("param")
                .Select(p => ("param_" + (p.Attribute("name")?.Value ?? "value"), "string", GetXmlText(p), false))
                .ToArray();

            var desc = new StringBuilder(string.IsNullOrWhiteSpace(summary) ? $"The {name} IOSP command." : summary);
            if (!string.IsNullOrWhiteSpace(remarks))
                desc.Append($" Remarks: {remarks}");
            desc.Append(" Call /api/docs for full workflow documentation.");

            yield return MakeTool(
                $"describe_{ToSnakeCase(name)}",
                desc.ToString(),
                BuildSchema(xmlParams));
        }
    }

    // ─── Tool Execution ─────────────────────────────────────────────────────────

    private static async Task<object> ExecuteToolCall(JsonObject? @params, IServiceProvider services, Assembly callingAssembly, HttpContext httpContext)
    {
        var toolName = @params?["name"]?.GetValue<string>() ?? "";
        var args = @params?["arguments"] as JsonObject ?? new JsonObject();

        if (toolName.StartsWith("describe_") || (toolName.StartsWith("api_") && toolName.EndsWith("_info")))
        {
            return SuccessContent("This is a descriptive tool. Visit /docs or /api/docs for full API documentation.");
        }

        // Check custom (WithSummary) endpoints first — before entity name resolution,
        // so tool names like "delete_api_v1_Customers_id" don't get misrouted.
        var endpointDs = services.GetRequiredService<EndpointDataSource>();
        var isCustomEndpoint = endpointDs.Endpoints.OfType<RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<CleanCodeEntityMetadata>() == null)
            .Any(e =>
            {
                var name = e.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
                var method = e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "GET";
                var route = e.RoutePattern.RawText ?? "";
                var expected = name != null ? ToSnakeCase(name) : $"{method.ToLowerInvariant()}_{RouteToSnakeCase(route)}";
                return expected == toolName;
            });

        if (isCustomEndpoint)
            return await ExecuteCustomEndpointTool(toolName, args, services, httpContext);

        var entityName = ResolveEntityNameFromToolName(toolName);
        if (entityName == null)
            return ErrorContent($"Unknown tool: {toolName}");

        return await ExecuteCrudToolViaHttp(toolName, entityName, args, services, httpContext);
    }

    internal static string? ResolveEntityNameFromToolName(string toolName)
    {
        if (toolName.StartsWith("list_"))
            return SnakeCaseToPascal(toolName["list_".Length..]);

        if (toolName.StartsWith("get_") && toolName.EndsWith("_by_id"))
            return SnakeCaseToPascal(toolName["get_".Length..^"_by_id".Length]);

        if (toolName.StartsWith("create_"))
            return SnakeCaseToPascal(toolName["create_".Length..]);

        if (toolName.StartsWith("update_"))
            return SnakeCaseToPascal(toolName["update_".Length..]);

        if (toolName.StartsWith("delete_"))
            return SnakeCaseToPascal(toolName["delete_".Length..]);

        return null;
    }

    private static async Task<object> ExecuteCrudToolViaHttp(string toolName, string entityName, JsonObject args, IServiceProvider services, HttpContext httpContext)
    {
        var (operation, httpMethod, needsId) = toolName switch
        {
            var n when n.StartsWith("list_")                          => ("LIST",      "GET",    false),
            var n when n.StartsWith("get_") && n.EndsWith("_by_id")  => ("GET_BY_ID", "GET",    true),
            var n when n.StartsWith("create_")                        => ("POST",      "POST",   false),
            var n when n.StartsWith("update_")                        => ("PUT",       "PUT",    false),
            var n when n.StartsWith("delete_")                        => ("DELETE",    "DELETE", true),
            _                                                         => (null,        null,     false)
        };

        if (operation == null)
            return ErrorContent($"Unknown CRUD operation for tool: {toolName}");

        var endpointDataSource = services.GetRequiredService<EndpointDataSource>();

        // 1. Minimal API: find via CleanCodeEntityMetadata.Operation
        var endpoint = endpointDataSource.Endpoints.OfType<RouteEndpoint>()
            .FirstOrDefault(e =>
            {
                var meta = e.Metadata.GetMetadata<CleanCodeEntityMetadata>();
                return meta?.EntityType.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase) == true
                    && meta.Operation == operation;
            });

        // 2. Controller fallback: match by HTTP method + entity name in route + {id} presence
        if (endpoint == null)
        {
            endpoint = endpointDataSource.Endpoints.OfType<RouteEndpoint>()
                .Where(e => e.Metadata.GetMetadata<CleanCodeEntityMetadata>() == null)
                .Where(e => e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains(httpMethod) == true)
                .Where(e => (e.RoutePattern.RawText ?? "").Contains(entityName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(e =>
                {
                    var hasIdParam = e.RoutePattern.Parameters.Any(p => p.Name.Equals("id", StringComparison.OrdinalIgnoreCase));
                    var route = e.RoutePattern.RawText ?? "";
                    if (!needsId && httpMethod == "GET")
                        return !hasIdParam && !route.Contains("paged") && !route.Contains("filtered");
                    return hasIdParam == needsId;
                });
        }

        if (endpoint == null)
            return ErrorContent($"No HTTP endpoint found for entity '{entityName}', operation '{operation}'");

        // Build URL
        var url = endpoint.RoutePattern.RawText ?? "";
        if (needsId)
            url = url.Replace("{id}", args["id"]?.GetValue<string>() ?? "");

        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var fullUrl = $"{baseUrl}/{url.TrimStart('/')}";

        using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
        using var client = new HttpClient(handler);
        ForwardAuthHeader(httpContext, client);

        HttpRequestMessage req;
        if (httpMethod is "GET" or "DELETE")
        {
            req = new HttpRequestMessage(new HttpMethod(httpMethod), fullUrl);
        }
        else
        {
            var body = new JsonObject();
            foreach (var kv in args) body[kv.Key] = kv.Value?.DeepClone();
            req = new HttpRequestMessage(new HttpMethod(httpMethod), fullUrl)
            {
                Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
            };
        }

        var response = await client.SendAsync(req);
        var responseBody = await response.Content.ReadAsStringAsync();
        return SuccessContent(responseBody);
    }

    private static async Task<object> ExecuteCustomEndpointTool(string toolName, JsonObject args, IServiceProvider services, HttpContext httpContext)
    {
        var endpointDataSource = services.GetRequiredService<EndpointDataSource>();

        var endpoint = endpointDataSource.Endpoints.OfType<RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<CleanCodeEntityMetadata>() == null)
            .FirstOrDefault(e =>
            {
                var name = e.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
                var method = e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "GET";
                var route = e.RoutePattern.RawText ?? "";
                var expected = name != null ? ToSnakeCase(name) : $"{method.ToLowerInvariant()}_{RouteToSnakeCase(route)}";
                return expected == toolName;
            });

        if (endpoint == null)
            return ErrorContent($"Unknown tool: {toolName}");

        var httpMethod = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "GET";
        var routeParams = endpoint.RoutePattern.Parameters.Select(p => p.Name).ToHashSet();

        // Build URL — fill route parameters from args
        var url = endpoint.RoutePattern.RawText ?? "";
        foreach (var param in endpoint.RoutePattern.Parameters)
            url = url.Replace($"{{{param.Name}}}", args[param.Name]?.GetValue<string>() ?? "");

        // Remaining args → query string for GET, body for POST/PUT/PATCH
        var nonRouteArgs = args.Where(kv => !routeParams.Contains(kv.Key)).ToList();
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var fullUrl = $"{baseUrl}/{url.TrimStart('/')}";

        if ((httpMethod == "GET" || httpMethod == "DELETE") && nonRouteArgs.Count > 0)
        {
            var qs = string.Join("&", nonRouteArgs.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value?.GetValue<string>() ?? "")}"));
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
        else
        {
            var body = new JsonObject();
            foreach (var kv in nonRouteArgs) body[kv.Key] = kv.Value?.DeepClone();
            req = new HttpRequestMessage(new HttpMethod(httpMethod), fullUrl)
            {
                Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
            };
        }

        var response = await client.SendAsync(req);
        var responseBody = await response.Content.ReadAsStringAsync();
        return SuccessContent(responseBody);
    }

    private static void ForwardAuthHeader(HttpContext httpContext, HttpClient client)
    {
        if (httpContext.Request.Headers.TryGetValue("Authorization", out var auth))
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", (string?)auth);
    }

    internal static string RouteToSnakeCase(string route) =>
        string.Join("_", route.Replace("{", "").Replace("}", "")
            .Split('/', StringSplitOptions.RemoveEmptyEntries));

    // ─── Schema Builders ────────────────────────────────────────────────────────

    private static object MakeTool(string name, string description, object inputSchema) =>
        new { name, description, inputSchema };

    private static object BuildSchema(params (string name, string type, string description, bool required)[] props)
    {
        var properties = new Dictionary<string, object>();
        var required = new List<string>();
        foreach (var (n, t, d, r) in props)
        {
            properties[n] = new { type = t, description = d };
            if (r) required.Add(n);
        }
        return required.Count > 0
            ? (object)new { type = "object", properties, required }
            : new { type = "object", properties };
    }

    private static object BuildDtoSchema(Type dtoType)
    {
        var properties = new Dictionary<string, object>();
        var required = new List<string>();

        foreach (var prop in dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite))
        {
            var camelName = JsonNamingPolicy.CamelCase.ConvertName(prop.Name);
            var description = GetXmlPropertyDescription(dtoType, prop) ?? $"{prop.Name} property.";
            properties[camelName] = new { type = GetJsonType(prop.PropertyType), description };
            if (prop.PropertyType.IsValueType && Nullable.GetUnderlyingType(prop.PropertyType) == null)
                required.Add(camelName);
        }

        return required.Count > 0
            ? (object)new { type = "object", properties, required }
            : new { type = "object", properties };
    }

    internal static string GetJsonType(Type type)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        if (t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte)) return "integer";
        if (t == typeof(double) || t == typeof(float) || t == typeof(decimal)) return "number";
        if (t == typeof(bool)) return "boolean";
        if (t.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(t)) return "array";
        if (t.IsArray) return "array";
        return "string";
    }

    // ─── Reflection Helpers ─────────────────────────────────────────────────────

    private static bool IsSubclassOfGeneric(Type type, Type generic)
    {
        while (type != null && type != typeof(object))
        {
            var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (generic == cur) return true;
            type = type.BaseType!;
        }
        return false;
    }

    private static Type? GetGenericBaseType(Type type, Type generic)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == generic)
                return type;
            type = type.BaseType!;
        }
        return null;
    }

    private static void SetProp(object obj, string propName, object? value) =>
        obj.GetType().GetProperty(propName)?.SetValue(obj, value);

    internal static string ToSnakeCase(string name)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0) sb.Append('_');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }

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
                        membersRoot.Add(new XElement(m));
                }
            }
            catch { }
        }

        return merged;
    }

    private static string? GetXmlPropertyDescription(Type dtoType, PropertyInfo prop)
    {
        var xml = XmlDocs.Value;
        if (xml == null) return null;
        var memberName = $"P:{dtoType.FullName}.{prop.Name}";
        var member = xml.Descendants("member")
            .FirstOrDefault(x => x.Attribute("name")?.Value == memberName);
        var text = member == null ? null : GetXmlText(member.Element("summary"));
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static string GetXmlText(XElement? element)
    {
        if (element == null) return string.Empty;
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
