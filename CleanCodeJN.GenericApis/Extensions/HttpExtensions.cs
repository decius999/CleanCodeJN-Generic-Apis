using CleanCodeJN.GenericApis.Abstractions.Responses;

namespace CleanCodeJN.GenericApis.Extensions;
public static class HttpExtensions
{
    /// <summary>
    /// Maps the external HTTP result from an internal ResultEnum
    /// </summary>
    /// <param name="response">Clean Code Response</param>
    /// <returns>HTTP Result</returns>
    public static IResult AsHttpResult(this Response response) => response.ResultState.ToIResult(response);

    /// <summary>
    /// Maps the external HTTP result from an internal ResultEnum
    /// </summary>
    /// <typeparam name="T">TEntity</typeparam>
    /// <typeparam name="K">TDto</typeparam>
    /// <param name="response">Clean Code BaseResponse</param>
    /// <param name="dto">DTO Object</param>
    /// <returns>HTTP Result</returns>
    public static IResult AsHttpResult<T, K>(this BaseResponse<T> response, K dto)
          where T : class => response.ResultState.ToIResult(response, dto);

    /// <summary>
    /// Maps the external HTTP result from an internal ResultEnum
    /// </summary>
    /// <typeparam name="T">TEntity</typeparam>
    /// <typeparam name="K">TDto</typeparam>
    /// <param name="response">Clean Code BaseListResponse</param>
    /// <param name="dto">DTO Object</param>
    /// <returns>HTTP Result</returns>
    public static IResult AsHttpResult<T, K>(this BaseListResponse<T> response, K dto) => response.ResultState.ToIResult(response, dto);
}
