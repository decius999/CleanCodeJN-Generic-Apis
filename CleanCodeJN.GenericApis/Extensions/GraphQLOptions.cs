namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Represents configuration options for automatically generating GraphQL endpoints for entities.
/// </summary>
/// <remarks>These options allow enabling or disabling the automatic creation of GraphQL endpoints for common CRUD
/// operations (GET, POST, PUT, DELETE) on entities. Each property corresponds to a specific operation and determines
/// whether the endpoint for that operation is generated.</remarks>
public class GraphQLOptions
{
    /// <summary>
    /// Add automatic GraphQL GET endpoint to every entity
    /// </summary>
    public bool Get { get; set; }

    /// <summary>
    /// Add automatic GraphQL POST endpoint to every entity
    /// </summary>
    public bool Create { get; set; }

    /// <summary>
    /// Add automatic GraphQL PUT endpoint to every entity
    /// </summary>
    public bool Update { get; set; }

    /// <summary>
    /// Add automatic GraphQL DELETE endpoint to every entity
    /// </summary>
    public bool Delete { get; set; }
}
