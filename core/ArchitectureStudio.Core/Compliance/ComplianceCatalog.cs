using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchitectureStudio.Core;

public sealed class ComplianceCatalog
{
    private const string RelativeControlsDirectory = "Compliance/Controls";
    private const string RelativeRegulationsDirectory = "Compliance/Regulations";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
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

        return new ComplianceCatalog(
            controls.ToDictionary(static control => control.Id, StringComparer.OrdinalIgnoreCase),
            regulations);
    }

    private static IReadOnlyList<ComplianceControlDefinition> ReadControls(string path)
    {
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<IReadOnlyList<ComplianceControlDefinition>>(stream, SerializerOptions)
            ?? [];
    }

    private static IReadOnlyList<ComplianceRegulationDefinition> ReadRegulations(string path)
    {
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<IReadOnlyList<ComplianceRegulationDefinition>>(stream, SerializerOptions)
            ?? [];
    }
}
