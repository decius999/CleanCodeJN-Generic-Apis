using CleanCodeJN.GenericApis.Abstractions.Responses;

namespace CleanCodeJN.GenericApis.Extensions;

public static class ResultEnumExtensions
{
    public static bool Succeeded(this ResultEnum result) => (int)result < 300;

    public static IResult ToIResult(this ResultEnum result, object data) => result switch
    {
        ResultEnum.SUCCESS => Results.Ok(data),
        ResultEnum.SUCCESS_CREATED => Results.Created(),
        ResultEnum.SUCCESS_ACCEPTED => Results.Accepted(),
        ResultEnum.SUCCESS_NO_CONTENT => Results.NoContent(),
        ResultEnum.FAILURE_BAD_REQUEST => Results.BadRequest(data),
        ResultEnum.FAILURE_NOT_FOUND => Results.NotFound(data),
        ResultEnum.FAILURE_UNAUTHORIZED => Results.Unauthorized(),
        ResultEnum.FAILURE_FORBIDDEN => Results.Forbid(),
        _ => Results.BadRequest(data),
    };
}
