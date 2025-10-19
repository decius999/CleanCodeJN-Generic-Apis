using System.Reflection;
using System.Xml.Linq;
using Path = System.IO.Path;

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

        app.MapGet("/api/docs", () =>
        {
            var xmlPath = Path.ChangeExtension(Assembly.GetEntryAssembly().Location, ".Business.xml");

            if (!System.IO.File.Exists(xmlPath))
            {
                return Results.Json(new { commands = Array.Empty<object>() });
            }

            var xml = XDocument.Load(xmlPath);

            var commands = xml
            .Descendants("member")
            .Where(x => x.Attribute("name")?.Value.StartsWith("T:") == true && x.Attribute("name").Value.EndsWith("Command"))
            .Select(x => new
            {
                name = GetLastPart(x.Attribute("name")?.Value.Split(':')[1]),
                @namespace = GetWithoutLastPart(x.Attribute("name")?.Value.Split(':')[1]),
                summary = x.Element("summary")?.Value.Trim(),
                remarks = x.Element("remarks")?.Value.Trim(),
                steps = new[]
                {
                    new { title = "SetSubmissionStateOrInterruptRequest (PROCESSING_STARTED)", description = "Setzt Submission Status auf PROCESSING_STARTED." },
                    new { title = "SubmissionGetByIdRequest", description = "Lädt Submission anhand ID." },
                    new { title = "SendSubmissionSendEvent", description = "Sendet Completion Event." }
                }
            })
            .ToList();

            return Results.Json(new { commands });
        });

        return app;
    }

    private static string GetWithoutLastPart(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var index = input.LastIndexOf('.');
        return index > 0 ? input[..index] : input;
    }

    private static string GetLastPart(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var index = input.LastIndexOf('.');
        return index >= 0 && index < input.Length - 1 ? input[(index + 1)..] : input;
    }
}

