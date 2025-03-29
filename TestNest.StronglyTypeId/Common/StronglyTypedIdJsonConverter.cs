using System.Text.Json.Serialization;
using System.Text.Json;
using TestNest.Domain.Exceptions;

namespace TestNest.StronglyTypeId.Common;
public class StronglyTypedIdJsonConverter<T> : JsonConverter<T> where T : StronglyTypedId<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new StronglyTypedIdException.Json.Deserialization(typeof(T), reader.GetString());

        var value = reader.GetString();

        if (Guid.TryParse(value, out var guid))
        {
            var idInstance = Activator.CreateInstance(typeof(T), guid) as T;
            if (idInstance is null)
                throw new StronglyTypedIdException.Creation(typeof(T), StronglyTypedIdException.CreationFailureReason.NullInstance);

            return idInstance;
        }

        throw new StronglyTypedIdException.Json.Deserialization(typeof(T), value);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value.ToString());
    }
}

