using System.Net;
using CleanCodeJN.GenericApis.Abstractions.Responses;

namespace CleanCodeJN.GenericApis.Extensions;

public static class ResultEnumExtensions
{
    /// <summary>
    /// Checks if the result is a success
    /// </summary>
    /// <param name="result">The Result Enum</param>
    /// <returns>true: Succeded, else: false</returns>
    public static bool Succeeded(this ResultEnum result) => (int)result < 300;

    /// <summary>
    /// Maps the external HTTP result from an internal ResultEnum
    /// </summary>
    /// <param name="result">Result Enum</param>
    /// <param name="response">CleanCode Response</param>
    /// <param name="data">the data of the response</param>
    /// <returns>HTTP Result</returns>
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
            if (error.Contains("'"))
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
            else
            {
                var key = "Execution Context";
                if (!result.ContainsKey(key))
                {
                    result.Add(key, [error?.Trim()]);
                }
                else
                {
                    result[key].Add(error?.Trim());
                }
            }
        }

        return result.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }
}
