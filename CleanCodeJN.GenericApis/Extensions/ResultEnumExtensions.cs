using System.Net;
using CleanCodeJN.GenericApis.Abstractions.Responses;

namespace CleanCodeJN.GenericApis.Extensions;

public static class ResultEnumExtensions
{
    public static bool Succeeded(this ResultEnum result) => (int)result < 300;

    public static IResult ToIResult(this ResultEnum result, Response response, object data = null) => result switch
    {
        ResultEnum.SUCCESS => Results.Ok(data),
        ResultEnum.SUCCESS_CREATED => Results.Created(),
        ResultEnum.SUCCESS_ACCEPTED => Results.Accepted(),
        ResultEnum.SUCCESS_NO_CONTENT => Results.NoContent(),
        ResultEnum.FAILURE_BAD_REQUEST => Results.ValidationProblem(errors: Build(response), title: "One or more validation errors occurred.", type: "Bad Request", extensions: new Dictionary<string, object>() { { "errors", Build(response) } }),
        ResultEnum.FAILURE_NOT_FOUND => Results.Problem(detail: response.Message, statusCode: (int)HttpStatusCode.NotFound, title: nameof(HttpStatusCode.NotFound)),
        ResultEnum.FAILURE_UNAUTHORIZED => Results.Problem(statusCode: (int)HttpStatusCode.Unauthorized, title: nameof(HttpStatusCode.Unauthorized)),
        ResultEnum.FAILURE_FORBIDDEN => Results.Problem(statusCode: (int)HttpStatusCode.Forbidden, title: nameof(HttpStatusCode.Forbidden)),
        _ => Results.Problem(detail: response.Message ?? response.Info, statusCode: (int)HttpStatusCode.BadRequest, title: nameof(HttpStatusCode.BadRequest)),
    };

    private static Dictionary<string, string[]> Build(Response response) => response
        .Message
        .Split('.')
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .ToDictionary(k => k.Split("'")[1]?.Trim(), v => new List<string>() { v?.Trim() }.ToArray());
}
