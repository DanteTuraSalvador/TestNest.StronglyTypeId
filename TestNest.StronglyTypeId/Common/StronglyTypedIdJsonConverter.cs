using System.Text.Json; // Importing the JSON library for serialization and deserialization.
using System.Text.Json.Serialization; // Importing the library for custom JSON converters.
using TestNest.StronglyTypeId.Exceptions; // Bringing in our custom exceptions for strongly typed IDs.

namespace TestNest.StronglyTypeId.Common;

// This class is a custom JSON converter for strongly typed IDs.
// It tells the JSON serializer how to read (deserialize) and write (serialize) these IDs.
public class StronglyTypedIdJsonConverter<T> : JsonConverter<T> where T : StronglyTypedId<T>
{
    // This method is called when we read a JSON value and want to convert it into a strongly typed ID.
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // First, we check if the incoming JSON value is actually a string.
        // If it’s not, that’s an issue, so we throw a deserialization exception.
        if (reader.TokenType != JsonTokenType.String)
            throw StronglyTypedIdException.JsonDeserialization();

        // Read the actual value from the JSON input.
        var value = reader.GetString();

        // If the value is null, empty, or just whitespace, that's also a problem, so we throw an exception.
        if (string.IsNullOrWhiteSpace(value))
            throw StronglyTypedIdException.JsonDeserialization();

        // Now, we try to convert the string into a valid GUID (since our strongly typed IDs are based on GUIDs).
        if (Guid.TryParse(value, out var guid))
        {
            // If it's a valid GUID, we create an instance of the strongly typed ID type `T`.
            var idInstance = Activator.CreateInstance(typeof(T), guid) as T;

            // If for some reason the instance creation fails, we throw an exception to signal this issue.
            if (idInstance is null)
                throw StronglyTypedIdException.NullInstance();

            // Everything went well! We return the newly created strongly typed ID.
            return idInstance;
        }

        // If we couldn't parse the string into a valid GUID, we throw another deserialization error.
        throw StronglyTypedIdException.JsonDeserialization();
    }

    // This method is called when we want to convert a strongly typed ID into JSON.
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Since our strongly typed IDs are just GUIDs under the hood, we write them as simple strings in JSON.
        writer.WriteStringValue(value.Value.ToString());
    }
}