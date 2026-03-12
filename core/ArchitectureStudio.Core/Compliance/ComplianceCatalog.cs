using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchitectureStudio.Core;

public sealed class ComplianceCatalog
{
    private const string RelativeControlsDirectory = "Compliance/Controls";
    private const string RelativeRegulationsDirectory = "Compliance/Regulations";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private readonly IReadOnlyDictionary<string, ComplianceControlDefinition> _controlsById;
    private readonly IReadOnlyList<ComplianceRegulationDefinition> _regulations;

    static ComplianceCatalog()
    {
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    private ComplianceCatalog(
        IReadOnlyDictionary<string, ComplianceControlDefinition> controlsById,
        IReadOnlyList<ComplianceRegulationDefinition> regulations)
    {
        _controlsById = controlsById;
        _regulations = regulations;
    }

    public IReadOnlyDictionary<string, ComplianceControlDefinition> ControlsById => _controlsById;

    public IReadOnlyList<ComplianceRegulationDefinition> Regulations => _regulations;

    public static ComplianceCatalog CreateDefault()
    {
        return StudioRuntimeCatalogFactory.CreateDefault().ComplianceCatalog;
    }

    internal static ComplianceCatalog CreateBuiltIn()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var controlsDirectory = Path.Combine(baseDirectory, RelativeControlsDirectory);
        var regulationsDirectory = Path.Combine(baseDirectory, RelativeRegulationsDirectory);

        if (!Directory.Exists(controlsDirectory))
        {
            throw new InvalidOperationException($"Compliance controls directory '{controlsDirectory}' was not found.");
        }

        if (!Directory.Exists(regulationsDirectory))
        {
            throw new InvalidOperationException($"Compliance regulations directory '{regulationsDirectory}' was not found.");
        }

        var controls = Directory.GetFiles(controlsDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .SelectMany(ReadControls)
            .ToArray();
        var regulations = Directory.GetFiles(regulationsDirectory, "*.json", SearchOption.TopDirectoryOnly)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .SelectMany(ReadRegulations)
            .OrderBy(static regulation => regulation.Title, StringComparer.Ordinal)
            .ThenBy(static regulation => regulation.Id, StringComparer.Ordinal)
            .ToArray();

        ValidateCatalog(controls, regulations);

        return CreateCatalog(controls, regulations);
    }

    internal ComplianceCatalog WithExternalPackage(ExternalPackage package)
    {
        var controls = package.Contributions.Controls
            .SelectMany(reference => ReadControls(reference.FullPath))
            .ToArray();
        var regulations = package.Contributions.Regulations
            .SelectMany(reference => ReadRegulations(reference.FullPath))
            .ToArray();

        return WithDefinitions(controls, regulations);
    }

    private static IReadOnlyList<ComplianceControlDefinition> ReadControls(string path)
    {
        return ReadJsonValues<ComplianceControlDefinition>(path);
    }

    private static IReadOnlyList<ComplianceRegulationDefinition> ReadRegulations(string path)
    {
        return ReadJsonValues<ComplianceRegulationDefinition>(path);
    }

    private ComplianceCatalog WithDefinitions(
        IReadOnlyList<ComplianceControlDefinition> controls,
        IReadOnlyList<ComplianceRegulationDefinition> regulations)
    {
        var mergedControls = _controlsById
            .Values
            .Concat(controls)
            .GroupBy(static control => control.Id, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.Last())
            .OrderBy(static control => control.Id, StringComparer.Ordinal)
            .ToArray();
        var mergedRegulations = _regulations
            .Concat(regulations)
            .GroupBy(static regulation => regulation.Id, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.Last())
            .OrderBy(static regulation => regulation.Title, StringComparer.Ordinal)
            .ThenBy(static regulation => regulation.Id, StringComparer.Ordinal)
            .ToArray();

        return CreateCatalog(mergedControls, mergedRegulations);
    }

    private static IReadOnlyList<T> ReadJsonValues<T>(string path)
    {
        using var stream = File.OpenRead(path);
        using var document = JsonDocument.Parse(stream);

        return document.RootElement.ValueKind switch
        {
            JsonValueKind.Array => document.RootElement.Deserialize<IReadOnlyList<T>>(SerializerOptions) ?? [],
            JsonValueKind.Object => document.RootElement.Deserialize<T>(SerializerOptions) is { } value ? [value] : [],
            _ => []
        };
    }

    private static void ValidateCatalog(
        IReadOnlyList<ComplianceControlDefinition> controls,
        IReadOnlyList<ComplianceRegulationDefinition> regulations)
    {
        foreach (var control in controls)
        {
            if (string.IsNullOrWhiteSpace(control.Id)
                || string.IsNullOrWhiteSpace(control.Title)
                || string.IsNullOrWhiteSpace(control.RemediationTitle)
                || string.IsNullOrWhiteSpace(control.RemediationSummary))
            {
                throw new InvalidOperationException("Compliance control library contains an invalid control definition.");
            }
        }

        var controlIds = controls.Select(static control => control.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var regulation in regulations)
        {
            if (string.IsNullOrWhiteSpace(regulation.Id)
                || string.IsNullOrWhiteSpace(regulation.Jurisdiction)
                || regulation.RequiredControls.Count == 0
                || regulation.DataTypes.Count == 0)
            {
                throw new InvalidOperationException($"Compliance regulation '{regulation.Id}' is missing required schema fields.");
            }

            foreach (var controlId in regulation.RequiredControls)
            {
                if (!controlIds.Contains(controlId))
                {
                    throw new InvalidOperationException(
                        $"Compliance regulation '{regulation.Id}' references unknown control '{controlId}'.");
                }
            }
        }
    }

    private static ComplianceCatalog CreateCatalog(
        IReadOnlyList<ComplianceControlDefinition> controls,
        IReadOnlyList<ComplianceRegulationDefinition> regulations)
    {
        ValidateCatalog(controls, regulations);

        return new ComplianceCatalog(
            controls.ToDictionary(static control => control.Id, StringComparer.OrdinalIgnoreCase),
            regulations);
    }
}
