namespace CleanCodeJN.GenericApis.Abstractions.Responses;

/// <summary>
/// Represents HTTP-status-based result codes for API responses, covering the full range of
/// informational, success, redirection and failure status codes defined by the HTTP specification.
/// </summary>
public enum ResultEnum
{
    // 1xx Informational

    /// <summary>The server has received the request headers and the client should proceed to send the request body.</summary>
    INFORMATIONAL_CONTINUE = 100,

    /// <summary>The server is switching protocols as requested by the client.</summary>
    INFORMATIONAL_SWITCHING_PROTOCOLS = 101,

    /// <summary>The server has received and is processing the request, but no response is available yet.</summary>
    INFORMATIONAL_PROCESSING = 102,

    /// <summary>Used to return some response headers before the final response.</summary>
    INFORMATIONAL_EARLY_HINTS = 103,

    // 2xx Success

    /// <summary>The request succeeded.</summary>
    SUCCESS = 200,

    /// <summary>The request succeeded and a new resource was created.</summary>
    SUCCESS_CREATED = 201,

    /// <summary>The request was accepted for processing but processing has not been completed.</summary>
    SUCCESS_ACCEPTED = 202,

    /// <summary>The returned metadata is from a copy rather than the origin server.</summary>
    SUCCESS_NON_AUTHORITATIVE_INFORMATION = 203,

    /// <summary>The request succeeded but there is no content to return.</summary>
    SUCCESS_NO_CONTENT = 204,

    /// <summary>The request succeeded and the client should reset the document view.</summary>
    SUCCESS_RESET_CONTENT = 205,

    /// <summary>The server is delivering only part of the resource due to a range request.</summary>
    SUCCESS_PARTIAL_CONTENT = 206,

    /// <summary>The message body contains multiple separate status codes (WebDAV).</summary>
    SUCCESS_MULTI_STATUS = 207,

    /// <summary>The members of a DAV binding have already been enumerated and are not being included again (WebDAV).</summary>
    SUCCESS_ALREADY_REPORTED = 208,

    /// <summary>The server fulfilled a GET request and the response is a representation of the result of applied instance manipulations.</summary>
    SUCCESS_IM_USED = 226,

    // 3xx Redirection

    /// <summary>The request has more than one possible response.</summary>
    REDIRECT_MULTIPLE_CHOICES = 300,

    /// <summary>The requested resource has been permanently moved to a new URL.</summary>
    REDIRECT_MOVED_PERMANENTLY = 301,

    /// <summary>The requested resource resides temporarily under a different URL.</summary>
    REDIRECT_FOUND = 302,

    /// <summary>The response can be found under a different URL using a GET request.</summary>
    REDIRECT_SEE_OTHER = 303,

    /// <summary>The resource has not been modified since the version specified by the request headers.</summary>
    REDIRECT_NOT_MODIFIED = 304,

    /// <summary>The requested resource must be accessed through the proxy given by the Location field.</summary>
    REDIRECT_USE_PROXY = 305,

    /// <summary>The requested resource resides temporarily under a different URL and the same method must be used.</summary>
    REDIRECT_TEMPORARY_REDIRECT = 307,

    /// <summary>The requested resource has been permanently moved and the same method must be used.</summary>
    REDIRECT_PERMANENT_REDIRECT = 308,

    // 4xx Client errors

    /// <summary>The request was malformed or contained invalid data.</summary>
    FAILURE_BAD_REQUEST = 400,

    /// <summary>The request requires authentication that was not provided or was invalid.</summary>
    FAILURE_UNAUTHORIZED = 401,

    /// <summary>Payment is required to access the requested resource.</summary>
    FAILURE_PAYMENT_REQUIRED = 402,

    /// <summary>The server understood the request but refuses to authorize it.</summary>
    FAILURE_FORBIDDEN = 403,

    /// <summary>The requested resource could not be found.</summary>
    FAILURE_NOT_FOUND = 404,

    /// <summary>The request method is not supported for the requested resource.</summary>
    FAILURE_METHOD_NOT_ALLOWED = 405,

    /// <summary>The requested resource cannot produce a response matching the Accept headers.</summary>
    FAILURE_NOT_ACCEPTABLE = 406,

    /// <summary>The client must first authenticate itself with the proxy.</summary>
    FAILURE_PROXY_AUTHENTICATION_REQUIRED = 407,

    /// <summary>The server timed out waiting for the request.</summary>
    FAILURE_REQUEST_TIMEOUT = 408,

    /// <summary>The request conflicts with the current state of the server.</summary>
    FAILURE_CONFLICT = 409,

    /// <summary>The requested resource is no longer available and will not be available again.</summary>
    FAILURE_GONE = 410,

    /// <summary>The request did not specify the length of its content, which is required by the resource.</summary>
    FAILURE_LENGTH_REQUIRED = 411,

    /// <summary>One or more preconditions given in the request header fields evaluated to false.</summary>
    FAILURE_PRECONDITION_FAILED = 412,

    /// <summary>The request entity is larger than limits defined by the server.</summary>
    FAILURE_REQUEST_ENTITY_TOO_LARGE = 413,

    /// <summary>The request URI is longer than the server is willing to interpret.</summary>
    FAILURE_REQUEST_URI_TOO_LONG = 414,

    /// <summary>The media format of the requested data is not supported by the server.</summary>
    FAILURE_UNSUPPORTED_MEDIA_TYPE = 415,

    /// <summary>The range specified by the Range header field cannot be fulfilled.</summary>
    FAILURE_REQUESTED_RANGE_NOT_SATISFIABLE = 416,

    /// <summary>The expectation given in the Expect request header field could not be met.</summary>
    FAILURE_EXPECTATION_FAILED = 417,

    /// <summary>The request was directed at a server that is not able to produce a response.</summary>
    FAILURE_MISDIRECTED_REQUEST = 421,

    /// <summary>The request was well-formed but unable to be followed due to semantic errors.</summary>
    FAILURE_UNPROCESSABLE_ENTITY = 422,

    /// <summary>The resource that is being accessed is locked (WebDAV).</summary>
    FAILURE_LOCKED = 423,

    /// <summary>The request failed because it depended on another request that failed (WebDAV).</summary>
    FAILURE_FAILED_DEPENDENCY = 424,

    /// <summary>The client should switch to a different protocol given in the Upgrade header field.</summary>
    FAILURE_UPGRADE_REQUIRED = 426,

    /// <summary>The origin server requires the request to be conditional.</summary>
    FAILURE_PRECONDITION_REQUIRED = 428,

    /// <summary>The user has sent too many requests in a given amount of time.</summary>
    FAILURE_TOO_MANY_REQUESTS = 429,

    /// <summary>The server is unwilling to process the request because its header fields are too large.</summary>
    FAILURE_REQUEST_HEADER_FIELDS_TOO_LARGE = 431,

    /// <summary>The requested resource is unavailable for legal reasons.</summary>
    FAILURE_UNAVAILABLE_FOR_LEGAL_REASONS = 451,

    // 5xx Server errors

    /// <summary>An unexpected error occurred on the server.</summary>
    FAILURE_INTERNAL_SERVER_ERROR = 500,

    /// <summary>The server does not support the functionality required to fulfill the request.</summary>
    FAILURE_NOT_IMPLEMENTED = 501,

    /// <summary>The server, while acting as a gateway or proxy, received an invalid response.</summary>
    FAILURE_BAD_GATEWAY = 502,

    /// <summary>The server is not ready to handle the request, often due to maintenance or overload.</summary>
    FAILURE_SERVICE_UNAVAILABLE = 503,

    /// <summary>The server, while acting as a gateway or proxy, did not get a response in time.</summary>
    FAILURE_GATEWAY_TIMEOUT = 504,

    /// <summary>The HTTP version used in the request is not supported by the server.</summary>
    FAILURE_HTTP_VERSION_NOT_SUPPORTED = 505,

    /// <summary>The server has an internal configuration error during content negotiation.</summary>
    FAILURE_VARIANT_ALSO_NEGOTIATES = 506,

    /// <summary>The server is unable to store the representation needed to complete the request (WebDAV).</summary>
    FAILURE_INSUFFICIENT_STORAGE = 507,

    /// <summary>The server detected an infinite loop while processing the request (WebDAV).</summary>
    FAILURE_LOOP_DETECTED = 508,

    /// <summary>Further extensions to the request are required for the server to fulfill it.</summary>
    FAILURE_NOT_EXTENDED = 510,

    /// <summary>The client needs to authenticate to gain network access.</summary>
    FAILURE_NETWORK_AUTHENTICATION_REQUIRED = 511,
}
