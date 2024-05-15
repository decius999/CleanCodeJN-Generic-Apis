using CleanCodeJN.GenericApis.Abstractions.Responses;

namespace CleanCodeJN.GenericApis.Extensions;
public static class HttpExtensions
{
    public static IResult AsHttpResult(this Response response) => response.ResultState.ToIResult(null);

    public static IResult AsHttpResult<T, K>(this BaseResponse<T> response, K dto)
          where T : class => response.ResultState.ToIResult(dto);

    public static IResult AsHttpResult<T, K>(this BaseListResponse<T> response, K dto) => response.ResultState.ToIResult(dto);
}
