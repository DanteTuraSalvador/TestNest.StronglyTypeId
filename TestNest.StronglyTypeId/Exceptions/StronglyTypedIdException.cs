namespace TestNest.Domain.Exceptions;

public class StronglyTypedIdException : Exception
{
    // Base constructor for all strongly typed ID exceptions
    protected StronglyTypedIdException(string message) : base(message) { }

    // Different failure reasons for ID creation
    public enum CreationFailureReason
    {
        NullInstance,  // Couldn't create an instance (Activator.CreateInstance returned null)
        InvalidGuid    // Provided GUID is invalid
    }

    // Argument-related exceptions (e.g., missing or null values)
    public class Argument : StronglyTypedIdException
    {
        // Base constructor for argument errors
        protected Argument(string message) : base(message) { }

        // Thrown when a required argument is null
        public class Null : Argument
        {
            public Null(string paramName)
                : base($"The argument '{paramName}' cannot be null in a strongly typed ID operation.") { }
        }
    }

    // Exceptions related to ID creation
    public class Creation : StronglyTypedIdException
    {
        public Creation(Type idType, CreationFailureReason reason)
            : base(GetErrorMessage(idType, reason)) { }

        // Generates a friendly error message based on the failure reason
        private static string GetErrorMessage(Type idType, CreationFailureReason reason) => reason switch
        {
            CreationFailureReason.NullInstance => $"Failed to create an instance of {idType.Name}. Activator.CreateInstance returned null.",
            CreationFailureReason.InvalidGuid => $"Failed to create an instance of {idType.Name} from the provided GUID.",
            _ => $"Unknown error while creating an instance of {idType.Name}."
        };
    }

    // Exceptions related to invalid ID format
    public class Format : StronglyTypedIdException
    {
        public Format(string input)
            : base($"Invalid format for strongly typed ID: {input}") { }
    }

    // Handles JSON serialization and deserialization errors for strongly typed IDs
    public class Json : StronglyTypedIdException
    {
        // Base constructor for JSON-related errors
        protected Json(string message) : base(message) { }

        // Error when trying to deserialize a strongly typed ID from JSON
        public class Deserialization : Json
        {
            public Deserialization(Type idType, string? input)
                : base($"Couldn't deserialize {idType.Name} from JSON. Invalid format: '{input ?? "null"}'.") { }
        }

        // Error when trying to serialize a strongly typed ID to JSON
        public class Serialization : Json
        {
            public Serialization(Type idType)
                : base($"Failed to serialize {idType.Name} to JSON.") { }
        }
    }

    // Handles errors related to model binding for strongly typed IDs
    public class ModelBinder : StronglyTypedIdException
    {
        // Base constructor for model binding errors
        protected ModelBinder(string message) : base(message) { }

        // Error when the binding context is null (this shouldn't happen)
        public class NullBindingContext : ModelBinder
        {
            public NullBindingContext()
                : base("The binding context cannot be null in StronglyTypedIdModelBinder.") { }
        }

        // Error when no value is provided for the strongly typed ID in an API request
        public class MissingValue : ModelBinder
        {
            public MissingValue(string modelName)
                : base($"No value was provided for the strongly typed ID '{modelName}' in the request.") { }
        }

        // Error when the provided value isn't a valid GUID
        public class InvalidFormat : ModelBinder
        {
            public InvalidFormat(string modelName, string? value)
                : base($"The provided value '{value}' for '{modelName}' is not a valid GUID.") { }
        }

        // Error when the system fails to create an instance of a strongly typed ID from a GUID
        public class ModelCreationFailure : ModelBinder
        {
            public ModelCreationFailure(Type idType)
                : base($"Failed to create an instance of '{idType.Name}' from the provided GUID.") { }
        }
    }
}