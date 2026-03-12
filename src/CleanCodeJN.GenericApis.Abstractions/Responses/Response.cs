namespace CleanCodeJN.GenericApis.Abstractions.Responses;

/// <summary>
/// Represents the base API response containing result state, messaging, and flow-control metadata.
/// </summary>
public class Response
{
    /// <summary>
    /// Initializes a new instance of <see cref="Response"/> with default values.
    /// </summary>
    public Response()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Response"/> with the specified result state and optional metadata.
    /// </summary>
    /// <param name="resultState">The result status of the response.</param>
    /// <param name="message">An optional human-readable message describing the result.</param>
    /// <param name="delay">An optional delay associated with the response.</param>
    /// <param name="info">Optional additional information about the response.</param>
    /// <param name="count">The number of items affected or returned. Defaults to 0.</param>
    /// <param name="interrupt">Indicates whether processing should be interrupted after this response. Defaults to <c>false</c>.</param>
    public Response(ResultEnum resultState, string message = null, TimeSpan? delay = null, string info = null, int? count = 0, bool? interrupt = false)
    {
        ResultState = resultState;
        Message = message;
        Delay = delay;
        Info = info;
        Count = count.Value;
        Interrupt = interrupt.Value;
    }

    /// <summary>
    /// Gets a value indicating whether the response represents a successful result (HTTP status code below 300).
    /// </summary>
    public bool Succeeded => (int)ResultState < 300;

    /// <summary>
    /// Gets or sets the result status code of the response.
    /// </summary>
    public ResultEnum ResultState { get; set; }

    /// <summary>
    /// Gets or sets a human-readable message describing the result.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets an optional delay to apply before or after processing this response.
    /// </summary>
    public TimeSpan? Delay { get; set; }

    /// <summary>
    /// Gets or sets additional informational text associated with the response.
    /// </summary>
    public string Info { get; set; }

    /// <summary>
    /// Gets or sets the number of items affected or returned by the operation.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether further pipeline execution should be interrupted after this response.
    /// </summary>
    public bool Interrupt { get; set; }
}
