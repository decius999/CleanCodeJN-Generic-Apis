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

        app.MapGet("/api/docs", () => Results.Json(new
        {
            commands = new[] {
                new {
                    name = "SubmissionProcessingEventIntegrationCommand1",
                    @namespace = "OneApi.Business.CustomMailMessageCommands",
                    summary = "Steuert den kompletten Lifecycle eines Submission Processing Events.",
                    steps = new[]
                        {
                            new { title = "SetSubmissionStateOrInterruptRequest (PROCESSING_STARTED)", description = "Setzt Submission Status auf PROCESSING_STARTED." },
                            new { title = "SubmissionGetByIdRequest", description = "Lädt Submission anhand ID." },
                            new { title = "SendSubmissionSendEvent", description = "Sendet Completion Event." }
                        }
                    },
                 new {
                    name = "SubmissionProcessingEventIntegrationCommand2",
                    @namespace = "OneApi.Business.CustomMailMessageCommands",
                    summary = "Steuert den kompletten Lifecycle eines Submission Processing Events.",
                    steps = new[]
                        {
                            new { title = "SetSubmissionStateOrInterruptRequest (PROCESSING_STARTED)", description = "Setzt Submission Status auf PROCESSING_STARTED." },
                            new { title = "SubmissionGetByIdRequest", description = "Lädt Submission anhand ID." },
                            new { title = "SendSubmissionSendEvent", description = "Sendet Completion Event." }
                        }
                    },
                  new {
                    name = "SubmissionProcessingEventIntegrationCommand3",
                    @namespace = "OneApi.Business.CustomMailMessageCommands",
                    summary = "Steuert den kompletten Lifecycle eines Submission Processing Events.",
                    steps = new[]
                        {
                            new { title = "SetSubmissionStateOrInterruptRequest (PROCESSING_STARTED)", description = "Setzt Submission Status auf PROCESSING_STARTED." },
                            new { title = "SubmissionGetByIdRequest", description = "Lädt Submission anhand ID." },
                            new { title = "SendSubmissionSendEvent", description = "Sendet Completion Event." }
                        }
                    },
                }
        }));

        return app;
    }
}

