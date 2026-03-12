namespace CleanCodeJN.GenericApis.Abstractions.Responses;

/// <summary>
/// Represents a generic API response that carries a list of items of type <typeparamref name="T"/> along with result metadata.
/// </summary>
/// <typeparam name="T">The type of the items in the data list.</typeparam>
public class BaseListResponse<T> : Response
{
    /// <summary>
    /// Initializes a new instance of <see cref="BaseListResponse{T}"/> with default values.
    /// </summary>
    public BaseListResponse()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="BaseListResponse{T}"/> with the specified result state and optional metadata.
    /// </summary>
    /// <param name="resultState">The result status of the response.</param>
    /// <param name="data">The optional list of data items to include in the response.</param>
    /// <param name="message">An optional human-readable message describing the result.</param>
    /// <param name="count">The total count of matching items, used for pagination. Defaults to 0.</param>
    /// <param name="info">Optional additional information about the response.</param>
    /// <param name="interrupt">Indicates whether processing should be interrupted after this response.</param>
    public BaseListResponse(ResultEnum resultState, List<T> data = default, string message = null, int? count = 0, string info = null, bool interrupt = false) : base(resultState, message, count: count, info: info, interrupt: interrupt) => Data = data;

    /// <summary>
    /// Gets or sets the list of data items returned by the operation.
    /// </summary>
    public List<T> Data { get; set; }

    /// <summary>
    /// Creates a completed task containing a new <see cref="BaseListResponse{T}"/> with the specified result state and optional metadata.
    /// </summary>
    /// <param name="resultState">The result status of the response.</param>
    /// <param name="data">The optional list of data items to include in the response.</param>
    /// <param name="message">An optional human-readable message describing the result.</param>
    /// <param name="count">The total count of matching items, used for pagination. Defaults to 0.</param>
    /// <param name="info">Optional additional information about the response.</param>
    /// <param name="interrupt">Indicates whether processing should be interrupted after this response.</param>
    /// <returns>A task that resolves to the constructed <see cref="BaseListResponse{T}"/>.</returns>
    public static Task<BaseListResponse<T>> Create(ResultEnum resultState, List<T> data = default, string message = null, int? count = 0, string info = null, bool interrupt = false) => Task.FromResult(new BaseListResponse<T>(resultState, data, message, count, info, interrupt));

    /// <summary>
    /// Creates a completed task containing a new <see cref="BaseListResponse{T}"/> based on a boolean success flag.
    /// </summary>
    /// <param name="success">When <c>true</c>, the response uses <see cref="ResultEnum.SUCCESS"/>; otherwise <see cref="ResultEnum.FAILURE_BAD_REQUEST"/>.</param>
    /// <param name="data">The optional list of data items to include in the response.</param>
    /// <param name="message">An optional human-readable message describing the result.</param>
    /// <param name="count">The total count of matching items, used for pagination. Defaults to 0.</param>
    /// <param name="info">Optional additional information about the response.</param>
    /// <param name="interrupt">Indicates whether processing should be interrupted after this response.</param>
    /// <returns>A task that resolves to the constructed <see cref="BaseListResponse{T}"/>.</returns>
    public static Task<BaseListResponse<T>> Create(bool success, List<T> data = default, string message = null, int? count = 0, string info = null, bool interrupt = false) => Task.FromResult(new BaseListResponse<T>(success ? ResultEnum.SUCCESS : ResultEnum.FAILURE_BAD_REQUEST, data, message, count, info, interrupt));
}
