using CleanCodeJN.GenericApis.Commands;

namespace CleanCodeJN.GenericApis.Extensions;
public static class HttpExtensions
{
    public static IResult AsHttpResult(this Response response) => !response.Success ? Results.BadRequest(response) : Results.Ok();

    public static IResult AsHttpResult<T, K>(this BaseResponse<T> response, K dto)
          where T : class => response.Data == null && !response.Success
    ? Results.BadRequest(response)
            : response.Success && response.Data is MemoryStream ? dto as IResult : HttpResults(response, dto);

    public static IResult AsHttpResult<T, K>(this BaseListResponse<T> response, K dto) => response.Data?.Any() == false && !response.Success ? Results.BadRequest(response) : HttpResults(response, dto);

    private static IResult HttpResults<K>(Response response, K dto)
    {
        if (response.Success)
        {
            return Results.Ok(dto);
        }
        else if (!response.Success)
        {
            return Results.BadRequest(response);
        }

        return Results.NoContent();
    }
}
