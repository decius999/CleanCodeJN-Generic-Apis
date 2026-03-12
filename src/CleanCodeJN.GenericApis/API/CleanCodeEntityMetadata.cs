#nullable enable
namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Endpoint metadata that stores entity type information for MCP tool generation.
/// Attached by MinimalApiExtensions.Map* methods so McpExtensions can discover
/// entity types at runtime via the EndpointDataSource.
/// </summary>
public class CleanCodeEntityMetadata
{
    /// <summary>The entity type (e.g. Customer).</summary>
    public Type EntityType { get; }

    /// <summary>The GET/response DTO type (e.g. CustomerGetDto).</summary>
    public Type GetDtoType { get; }

    /// <summary>The write DTO type for POST/PUT (e.g. CustomerPostDto). Null for read operations.</summary>
    public Type? WriteType { get; }

    /// <summary>The primary key type (e.g. int, Guid). Null when not applicable (POST/PUT).</summary>
    public Type? KeyType { get; }

    /// <summary>The CRUD operation: LIST, LIST_PAGED, LIST_FILTERED, GET_BY_ID, POST, PUT, PATCH, DELETE.</summary>
    public string Operation { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CleanCodeEntityMetadata"/> with the specified type and operation information.
    /// </summary>
    /// <param name="entityType">The entity type associated with the endpoint.</param>
    /// <param name="getDtoType">The GET/response DTO type.</param>
    /// <param name="writeType">The write DTO type for POST/PUT, or null for read operations.</param>
    /// <param name="keyType">The primary key type, or null when not applicable.</param>
    /// <param name="operation">The CRUD operation name (e.g. LIST, GET_BY_ID, POST).</param>
    public CleanCodeEntityMetadata(Type entityType, Type getDtoType, Type? writeType, Type? keyType, string operation)
    {
        EntityType = entityType;
        GetDtoType = getDtoType;
        WriteType = writeType;
        KeyType = keyType;
        Operation = operation;
    }
}
