namespace CleanCodeJN.GenericApis.Extensions;

public static class DocumentationExtensions
{
    public static IApplicationBuilder UseCleanCodeJNDocumentation(this WebApplication app, string path)
    {
        app.MapGet($"{path}", async context =>
        {
            context.Response.ContentType = "text/html";
            var stream = typeof(DocumentationExtensions).Assembly.GetManifestResourceStream("CleanCodeJN.GenericApis.Docs.index.html");

            if (stream == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("documentation page not found.");
                return;
            }

            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();
            await context.Response.WriteAsync(html);
        });

        return app;
    }
}

