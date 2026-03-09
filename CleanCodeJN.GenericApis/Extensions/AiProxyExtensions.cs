using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CleanCodeJN.GenericApis.Models;
using CleanCodeJN.GenericApis.Services;

namespace CleanCodeJN.GenericApis.Extensions;

public static class AiProxyExtensions
{
    private static readonly JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static WebApplication UseCleanCodeJNWithAiChat(this WebApplication app)
    {
        app.UseCors("BlazorChat");

        app.MapPost("/ai/chat", async (HttpContext context, AiProxyService service, ILogger<AiProxyService> logger, CancellationToken cancellationToken) =>
        {
            var request = await context.Request.ReadFromJsonAsync<ChatRequest>(cancellationToken);
            if (request is null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var bearerToken = ExtractBearer(context);

            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";
            context.Response.Headers.Connection = "keep-alive";

            try
            {
                await foreach (var ev in service.StreamAsync(request, bearerToken, cancellationToken))
                {
                    var json = JsonSerializer.Serialize(ev, CamelCase);
                    await context.Response.WriteAsync($"data: {json}\n\n", Encoding.UTF8, cancellationToken);
                    await context.Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in /ai/chat stream");
                var errorEvent = JsonSerializer.Serialize(new ChatStreamEvent("error", Content: ex.Message));
                await context.Response.WriteAsync($"data: {errorEvent}\n\n", Encoding.UTF8, CancellationToken.None);
                await context.Response.Body.FlushAsync(CancellationToken.None);
            }

            await context.Response.WriteAsync("data: [DONE]\n\n", Encoding.UTF8, CancellationToken.None);
            await context.Response.Body.FlushAsync(CancellationToken.None);
        });

        // Simple test endpoint — call GET /ai/test to verify Claude connectivity
        app.MapGet("/ai/test", async (AiProxyService service) =>
        {
            var request = new ChatRequest([new ChatMessage("user", "Say: OK")]);
            var result = new System.Text.StringBuilder();
            await foreach (var ev in service.StreamAsync(request, null, CancellationToken.None))
            {
                if (ev.Type == "text")
                {
                    result.Append(ev.Content);
                }
            }

            return result.Length > 0 ? Results.Ok(result.ToString()) : Results.Problem("No response from Claude");
        });

        return app;
    }

    private static string ExtractBearer(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var auth))
        {
            var val = auth.ToString();
            if (val.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return val["Bearer ".Length..].Trim();
            }
        }

        return null;
    }
}
