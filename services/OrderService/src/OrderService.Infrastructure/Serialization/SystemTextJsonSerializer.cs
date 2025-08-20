using System.Text.Json;

namespace OrderService.Infrastructure.Serialization;

public sealed class SystemTextJsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public string Serialize<T>(T value) => JsonSerializer.Serialize(value, _options);
}
