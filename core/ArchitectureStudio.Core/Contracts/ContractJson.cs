using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchitectureStudio.Core;

public static class ContractJson
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    static ContractJson()
    {
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, SerializerOptions);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }
}
