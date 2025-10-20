using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace CleanCodeJN.GenericApis.Extensions;

public static class DocumentationExtensions
{
    public static IApplicationBuilder UseCleanCodeJNDocumentation(this WebApplication app, string path = "/docs")
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
            var xml = LoadMergedXmlDocs();
            var assemblyDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? AppContext.BaseDirectory)!;
            var projectRoot = FindProjectRoot(assemblyDir);

            if (projectRoot == null)
            {
                throw new DirectoryNotFoundException("No project or solution root found.");
            }

            var allCsFiles = Directory.GetFiles(projectRoot, "*.cs", SearchOption.AllDirectories).ToList();

            var commands = xml
            .Descendants("member")
            .Where(x => x.Attribute("name")?.Value.StartsWith("T:") == true &&
                        x.Attribute("name").Value.EndsWith("Command"))
            .Select(x => new
            {
                name = GetLastPart(x.Attribute("name")?.Value.Split(':')[1]),
                @namespace = GetWithoutLastPart(x.Attribute("name")?.Value.Split(':')[1]),
                summary = x.Element("summary")?.Value.Trim() ?? string.Empty,
                remarks = x.Element("remarks")?.Value.Trim() ?? string.Empty,
                steps = ExtractExecutionContextCalls(x.Attribute("name")?.Value.Split(':')[1], projectRoot, allCsFiles)
                        .Select(x => new
                        {
                            title = x,
                            description = $"{xml.Descendants("member").FirstOrDefault(y => y.Attribute("name").Value.Contains(x))?.Element("summary")?.Value?.Trim() ?? string.Empty} {xml.Descendants("member").FirstOrDefault(y => y.Attribute("name").Value.Contains(x))?.Element("remarks")?.Value?.Trim() ?? string.Empty}"
                        }).ToList()
            })
            .ToList();

            return Results.Json(new { commands });
        });

        return app;
    }

    private static XDocument LoadMergedXmlDocs()
    {
        var baseDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? AppContext.BaseDirectory)!;
        var xmlFiles = Directory.GetFiles(baseDir, "*.xml", SearchOption.TopDirectoryOnly);

        if (xmlFiles.Length == 0)
        {
            throw new FileNotFoundException($"No XML docs found in: {baseDir}");
        }

        var merged = new XDocument(new XElement("doc",
            new XElement("assembly", new XElement("name", "MergedDocs")),
            new XElement("members")
        ));

        var membersRoot = merged.Root!.Element("members")!;

        foreach (var file in xmlFiles)
        {
            try
            {
                var doc = XDocument.Load(file);
                var members = doc.Descendants("member");
                foreach (var m in members)
                {
                    if (!membersRoot.Elements("member").Any(e => (string?)e.Attribute("name") == (string?)m.Attribute("name")))
                    {
                        membersRoot.Add(new XElement(m));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"XML error in {file}: {ex.Message}");
            }
        }

        return merged;
    }

    private static string FindSourceFile(string fullClassName, string projectRoot, List<string> allCsFiles)
    {
        var foundFile = allCsFiles.FirstOrDefault(x => x.Contains(fullClassName.Split('.').Last() + ".cs"));

        return foundFile ?? throw new FileNotFoundException($"Class {fullClassName} could not be found.");
    }

    private static string FindProjectRoot(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            if (dir.GetFiles("*.sln").Any())
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }

    private static IEnumerable<string> ExtractExecutionContextCalls(string fullClassName, string projectRoot, List<string> allCsFiles)
    {
        var code = File.ReadAllText(FindSourceFile(fullClassName, projectRoot, allCsFiles));
        var handleBodyPattern = @"ExecutionContext[\s\S]*?\.Execute";
        var handleMatch = Regex.Match(code, handleBodyPattern);

        if (!handleMatch.Success)
        {
            yield break;
        }

        var handleBody = handleMatch.Value;
        var callPattern = @"\.\s*(?<method>[A-Za-z_][A-Za-z0-9_]*)\s*\(";
        var matches = Regex.Matches(handleBody, callPattern);

        foreach (Match match in matches)
        {
            var methodName = match.Groups["method"].Value;

            if (methodName.EndsWith("Request", StringComparison.OrdinalIgnoreCase)
                || methodName.EndsWith("Requests", StringComparison.OrdinalIgnoreCase))
            {
                yield return methodName;
            }
        }
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

