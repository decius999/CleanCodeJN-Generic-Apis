using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.Services;

/// <summary>
/// Default exception handler that catches unexpected system exceptions and returns a
/// <see cref="ProblemDetails"/> (RFC 7807) response with HTTP 500.
/// Business logic errors should never reach this handler — use <c>BaseResponse</c> with
/// the appropriate <c>ResultEnum</c> and let <c>AsHttpResult()</c> produce the correct HTTP response.
/// <br/><br/>
/// Derive from this class and register your implementation <em>before</em> <c>AddCleanCodeJN</c>
/// to override the default behaviour:
/// <code>
/// builder.Services.AddExceptionHandler&lt;MyExceptionHandler&gt;();
/// builder.Services.AddCleanCodeJN&lt;AppDbContext&gt;(...);
/// </code>
/// </summary>
public class CleanCodeExceptionHandler : IExceptionHandler
{
    /// <inheritdoc/>
    public virtual async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = exception.Message,
                Detail = exception.StackTrace,
                Status = (int)HttpStatusCode.InternalServerError,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            },
            cancellationToken);

        return true;
    }
}
