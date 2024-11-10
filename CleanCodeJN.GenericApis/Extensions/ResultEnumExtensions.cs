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
        ResultEnum.FAILURE_BAD_REQUEST => Results.ValidationProblem(errors: BuildErrors(response), title: "One or more validation errors occurred.", type: "Bad Request", extensions: new Dictionary<string, object>() { { "errors", BuildErrors(response) } }),
        ResultEnum.FAILURE_NOT_FOUND => Results.Problem(detail: response.Message, statusCode: (int)HttpStatusCode.NotFound, title: nameof(HttpStatusCode.NotFound)),
        ResultEnum.FAILURE_UNAUTHORIZED => Results.Problem(statusCode: (int)HttpStatusCode.Unauthorized, title: nameof(HttpStatusCode.Unauthorized)),
        ResultEnum.FAILURE_FORBIDDEN => Results.Problem(statusCode: (int)HttpStatusCode.Forbidden, title: nameof(HttpStatusCode.Forbidden)),
        _ => Results.Problem(detail: response.Message ?? response.Info, statusCode: (int)HttpStatusCode.BadRequest, title: nameof(HttpStatusCode.BadRequest)),
    };

    private static Dictionary<string, string[]> BuildErrors(Response response)
    {
        var result = new Dictionary<string, List<string>>();

        var errors = response.Message
            .Split('.')
            .Where(x => !string.IsNullOrWhiteSpace(x));

        foreach (var error in errors)
        {
            var key = error.Split("'")[1]?.Trim();
            if (!result.ContainsKey(key))
            {
                result.Add(key, [error?.Trim()]);
            }
            else
            {
                result[key].Add(error?.Trim());
            }
        }

        return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }
}
