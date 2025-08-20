namespace OrderService.Infrastructure.Serialization;

public interface IJsonSerializer
{
    string Serialize<T>(T value);
}
