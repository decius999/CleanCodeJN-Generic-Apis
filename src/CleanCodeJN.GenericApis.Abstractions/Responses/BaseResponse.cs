namespace CleanCodeJN.GenericApis.Abstractions.Responses;

/// <summary>
/// Represents a generic API response that carries a single data object of type <typeparamref name="T"/> along with result metadata.
/// </summary>
/// <typeparam name="T">The type of the data payload, which must be a reference type.</typeparam>
public class BaseResponse<T> : Response
    where T : class
{
    /// <summary>
    /// Initializes a new instance of <see cref="BaseResponse{T}"/> with default values.
    /// </summary>
    public BaseResponse()
    {
    }

    /// <summary>
    /// Gets or sets the data payload of the response.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="BaseResponse{T}"/> with the specified result state and optional metadata.
    /// </summary>
    /// <param name="resultState">The result status of the response.</param>
    /// <param name="data">The optional data payload to include in the response.</param>
    /// <param name="message">An optional human-readable message describing the result.</param>
    /// <param name="delay">An optional delay associated with the response.</param>
    /// <param name="info">Optional additional information about the response.</param>
    /// <param name="interrupt">Indicates whether processing should be interrupted after this response.</param>
    public BaseResponse(ResultEnum resultState, T data = default, string message = null, TimeSpan? delay = null, string info = null, bool interrupt = false)
        : base(resultState, message, delay, info, data != default ? 1 : 0, interrupt) => Data = data;

    /// <summary>
    /// Creates a completed task containing a new <see cref="BaseResponse{T}"/> with the specified result state and optional metadata.
    /// </summary>
    /// <param name="resultState">The result status of the response.</param>
    /// <param name="data">The optional data payload to include in the response.</param>
    /// <param name="message">An optional human-readable message describing the result.</param>
    /// <param name="delay">An optional delay associated with the response.</param>
    /// <param name="info">Optional additional information about the response.</param>
    /// <param name="interrupt">Indicates whether processing should be interrupted after this response.</param>
    /// <returns>A task that resolves to the constructed <see cref="BaseResponse{T}"/>.</returns>
    public static Task<BaseResponse<T>> Create(ResultEnum resultState, T data = default, string message = null, TimeSpan? delay = null, string info = null,
        bool interrupt = false) => Task.FromResult(new BaseResponse<T>(resultState, data, message, delay, info, interrupt));

    /// <summary>
    /// Creates a completed task containing a new <see cref="BaseResponse{T}"/> based on a boolean success flag.
    /// </summary>
    /// <param name="success">When <c>true</c>, the response uses <see cref="ResultEnum.SUCCESS"/>; otherwise <see cref="ResultEnum.FAILURE_BAD_REQUEST"/>.</param>
    /// <param name="data">The optional data payload to include in the response.</param>
    /// <param name="message">An optional human-readable message describing the result.</param>
    /// <param name="delay">An optional delay associated with the response.</param>
    /// <param name="info">Optional additional information about the response.</param>
    /// <param name="interrupt">Indicates whether processing should be interrupted after this response.</param>
    /// <returns>A task that resolves to the constructed <see cref="BaseResponse{T}"/>.</returns>
    public static Task<BaseResponse<T>> Create(bool success, T data = default, string message = null, TimeSpan? delay = null, string info = null,
      bool interrupt = false) => Task.FromResult(new BaseResponse<T>(success ? ResultEnum.SUCCESS : ResultEnum.FAILURE_BAD_REQUEST, data, message, delay, info, interrupt));
}
