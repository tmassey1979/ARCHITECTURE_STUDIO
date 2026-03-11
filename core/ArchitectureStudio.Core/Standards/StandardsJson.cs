using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchitectureStudio.Core;

public static class StandardsJson
{
    public static JsonSerializerOptions SerializerOptions { get; } = CreateSerializerOptions();

    public static T Deserialize<T>(Stream stream)
    {
        var value = JsonSerializer.Deserialize<T>(stream, SerializerOptions);

        return value ?? throw new InvalidOperationException($"Unable to deserialize standards payload into {typeof(T).Name}.");
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
