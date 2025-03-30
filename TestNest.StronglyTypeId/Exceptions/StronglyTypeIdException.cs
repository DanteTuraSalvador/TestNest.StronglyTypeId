namespace TestNest.StronglyTypeId.Exceptions;
public sealed class StronglyTypedIdException : Exception
{
    public enum ErrorCode
    {
        NullInstanceCreation,
        InvalidGuidCreation,
        InvalidFormat,
        JsonDeserializationFailed,
        JsonSerializationFailed,
        NullBindingContext,
        MissingValue,
        InvalidModelValue,
        ModelCreationFailed,
        NullId
    }

    private static readonly Dictionary<ErrorCode, string> ErrorMessages = new()
        {
            { ErrorCode.NullInstanceCreation, "Failed to create instance of {0}. Activator returned null." },
            { ErrorCode.InvalidGuidCreation, "Invalid GUID provided for {0}." },
            { ErrorCode.InvalidFormat, "Invalid format: {0}." },
            { ErrorCode.JsonDeserializationFailed, "Failed to deserialize {0} from JSON. Input: '{1}'." },
            { ErrorCode.JsonSerializationFailed, "Failed to serialize {0} to JSON." },
            { ErrorCode.NullBindingContext, "Binding context cannot be null." },
            { ErrorCode.MissingValue, "No value provided for {0}." },
            { ErrorCode.InvalidModelValue, "Invalid value '{1}' for {0}." },
            { ErrorCode.ModelCreationFailed, "Failed to create model from GUID for {0}." },
            { ErrorCode.NullId, "ID cannot be null or empty." }
        };

    public ErrorCode Code { get; }

    private StronglyTypedIdException(ErrorCode code, string message)
        : base(message)
    {
        Code = code;
    }

    public static StronglyTypedIdException NullInstanceCreation(Type idType) =>
        new(ErrorCode.NullInstanceCreation,
            string.Format(ErrorMessages[ErrorCode.NullInstanceCreation], idType.Name));

    public static StronglyTypedIdException InvalidGuidCreation(Type idType) =>
        new(ErrorCode.InvalidGuidCreation,
            string.Format(ErrorMessages[ErrorCode.InvalidGuidCreation], idType.Name));

    public static StronglyTypedIdException InvalidFormat(string input) =>
        new(ErrorCode.InvalidFormat,
            string.Format(ErrorMessages[ErrorCode.InvalidFormat], input));

    public static StronglyTypedIdException JsonDeserializationFailed(Type idType, string? input) =>
        new(ErrorCode.JsonDeserializationFailed,
            string.Format(ErrorMessages[ErrorCode.JsonDeserializationFailed], idType.Name, input ?? "null"));

    public static StronglyTypedIdException JsonSerializationFailed(Type idType) =>
        new(ErrorCode.JsonSerializationFailed,
            string.Format(ErrorMessages[ErrorCode.JsonSerializationFailed], idType.Name));

    public static StronglyTypedIdException NullBindingContext() =>
        new(ErrorCode.NullBindingContext, ErrorMessages[ErrorCode.NullBindingContext]);

    public static StronglyTypedIdException MissingValue(string modelName) =>
        new(ErrorCode.MissingValue,
            string.Format(ErrorMessages[ErrorCode.MissingValue], modelName));

    public static StronglyTypedIdException InvalidModelValue(string modelName, string? value) =>
        new(ErrorCode.InvalidModelValue,
            string.Format(ErrorMessages[ErrorCode.InvalidModelValue], modelName, value ?? "null"));

    public static StronglyTypedIdException ModelCreationFailed(Type idType) =>
        new(ErrorCode.ModelCreationFailed,
            string.Format(ErrorMessages[ErrorCode.ModelCreationFailed], idType.Name));

    public static StronglyTypedIdException NullId() =>
        new(ErrorCode.NullId, ErrorMessages[ErrorCode.NullId]);
}