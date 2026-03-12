using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchitectureStudio.Core;

public sealed class ProjectTemplateCatalog
{
    private const string RelativeTemplatesDirectory = "Templates";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private readonly IReadOnlyList<ProjectTemplateDefinition> _templates;

    static ProjectTemplateCatalog()
    {
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    private ProjectTemplateCatalog(IReadOnlyList<ProjectTemplateDefinition> templates)
    {
        _templates = templates;
    }

    public IReadOnlyList<ProjectTemplateDefinition> Templates => _templates;

    public static ProjectTemplateCatalog CreateDefault()
    {
        return StudioRuntimeCatalogFactory.CreateDefault().ProjectTemplateCatalog;
    }

    internal static ProjectTemplateCatalog CreateBuiltIn()
    {
        var templatesDirectory = Path.Combine(AppContext.BaseDirectory, RelativeTemplatesDirectory);
        if (!Directory.Exists(templatesDirectory))
        {
            throw new InvalidOperationException($"Template directory '{templatesDirectory}' was not found.");
        }

        var templates = Directory.GetFiles(templatesDirectory, "*.json", SearchOption.AllDirectories)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .Select(ReadTemplate)
            .OrderBy(static template => template.Id, StringComparer.Ordinal)
            .ToArray();

        ValidateTemplates(templates);

        return new ProjectTemplateCatalog(templates);
    }

    internal ProjectTemplateCatalog WithExternalPackage(ExternalPackage package)
    {
        var templates = package.Contributions.Templates
            .Select(reference => ReadTemplate(reference.FullPath))
            .ToArray();

        return WithTemplates(templates);
    }

    private static ProjectTemplateDefinition ReadTemplate(string path)
    {
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<ProjectTemplateDefinition>(stream, SerializerOptions)
            ?? throw new InvalidOperationException($"Template '{path}' could not be deserialized.");
    }

    private ProjectTemplateCatalog WithTemplates(IReadOnlyList<ProjectTemplateDefinition> templates)
    {
        var mergedTemplates = _templates
            .Concat(templates)
            .GroupBy(static template => template.Id, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.Last())
            .OrderBy(static template => template.Id, StringComparer.Ordinal)
            .ToArray();

        ValidateTemplates(mergedTemplates);
        return new ProjectTemplateCatalog(mergedTemplates);
    }

    private static void ValidateTemplates(IReadOnlyList<ProjectTemplateDefinition> templates)
    {
        foreach (var template in templates)
        {
            if (string.IsNullOrWhiteSpace(template.Id)
                || string.IsNullOrWhiteSpace(template.SelectionKey)
                || template.Files.Count == 0)
            {
                throw new InvalidOperationException("Project template catalog contains an invalid template definition.");
            }

            if (!template.SelectionKey.Equals("always", StringComparison.OrdinalIgnoreCase)
                && template.SelectionValues.Count == 0)
            {
                throw new InvalidOperationException($"Project template '{template.Id}' does not declare selection values.");
            }
        }
    }
}
