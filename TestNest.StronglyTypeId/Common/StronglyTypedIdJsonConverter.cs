using System.Text.Json.Serialization;
using System.Text.Json;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.Common;

public class StronglyTypedIdJsonConverter<T> : JsonConverter<T> where T : StronglyTypedId<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw StronglyTypedIdException.JsonDeserializationFailed(
                typeof(T),
                reader.TokenType.ToString()
            );
        }

        var value = reader.GetString();

        if (Guid.TryParse(value, out var guid))
        {
            var idInstance = Activator.CreateInstance(typeof(T), guid) as T;
            return idInstance ?? throw StronglyTypedIdException.NullInstanceCreation(typeof(T));
        }

        throw StronglyTypedIdException.JsonDeserializationFailed(
            typeof(T),
            value ?? "null"
        );
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.Value.ToString());
        }
    }
}