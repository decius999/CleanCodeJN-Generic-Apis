namespace CleanCodeJN.GenericApis.Abstractions.Responses;

/// <summary>
/// Represents HTTP-status-based result codes for API responses, covering success and common failure scenarios.
/// </summary>
public enum ResultEnum
{
    /// <summary>The request succeeded.</summary>
    SUCCESS = 200,

    /// <summary>The request succeeded and a new resource was created.</summary>
    SUCCESS_CREATED = 201,

    /// <summary>The request was accepted for processing but processing has not been completed.</summary>
    SUCCESS_ACCEPTED = 202,

    /// <summary>The request succeeded but there is no content to return.</summary>
    SUCCESS_NO_CONTENT = 204,

    /// <summary>The request was malformed or contained invalid data.</summary>
    FAILURE_BAD_REQUEST = 400,

    /// <summary>The request requires authentication that was not provided or was invalid.</summary>
    FAILURE_UNAUTHORIZED = 401,

    /// <summary>The server understood the request but refuses to authorize it.</summary>
    FAILURE_FORBIDDEN = 403,

    /// <summary>The requested resource could not be found.</summary>
    FAILURE_NOT_FOUND = 404,

    /// <summary>An unexpected error occurred on the server.</summary>
    FAILURE_INTERNAL_SERVER_ERROR = 500,
}
