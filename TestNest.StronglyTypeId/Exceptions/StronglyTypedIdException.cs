namespace TestNest.StronglyTypeId.Exceptions;

public class StronglyTypedIdException : Exception
{
    public enum ErrorCode
    {
        NullArgument,
        NullInstance,
        InvalidGuid,
        JsonDeserialization,
        JsonSerialization,
        ModelBindingNullContext,
        ModelBindingMissingValue,
        ModelBindingInvalidFormat,
        ModelBindingCreationFailure,
        InvalidComparison,
        NullId
    }

    private static readonly Dictionary<ErrorCode, string> ErrorMessages = new()
    {
        { ErrorCode.NullArgument, "The argument cannot be null in a strongly typed ID operation." },
        { ErrorCode.NullInstance, "Failed to create an instance. Activator.CreateInstance returned null." },
        { ErrorCode.InvalidGuid, "The provided value is not a valid GUID." },
        { ErrorCode.JsonDeserialization, "Couldn't deserialize from JSON. Invalid format." },
        { ErrorCode.JsonSerialization, "Failed to serialize to JSON." },
        { ErrorCode.ModelBindingNullContext, "The binding context cannot be null in StronglyTypedIdModelBinder." },
        { ErrorCode.ModelBindingMissingValue, "No value was provided for the strongly typed ID in the request." },
        { ErrorCode.ModelBindingInvalidFormat, "The provided value for the strongly typed ID is not a valid GUID." },
        { ErrorCode.ModelBindingCreationFailure, "Failed to create an instance of the strongly typed ID from the provided GUID." },
        { ErrorCode.InvalidComparison, "Invalid comparison between incompatible types." },
        { ErrorCode.NullId, "ID cannot be null or empty." }
    };

    public ErrorCode Code { get; }

    public StronglyTypedIdException(ErrorCode code) : base(ErrorMessages[code])
    {
        Code = code;
    }

    // Static helper methods
    public static StronglyTypedIdException NullArgument() => new(ErrorCode.NullArgument);
    public static StronglyTypedIdException NullInstance() => new(ErrorCode.NullInstance);
    public static StronglyTypedIdException InvalidGuid() => new(ErrorCode.InvalidGuid);
    public static StronglyTypedIdException JsonDeserialization() => new(ErrorCode.JsonDeserialization);
    public static StronglyTypedIdException JsonSerialization() => new(ErrorCode.JsonSerialization);
    public static StronglyTypedIdException ModelBindingNullContext() => new(ErrorCode.ModelBindingNullContext);
    public static StronglyTypedIdException ModelBindingMissingValue() => new(ErrorCode.ModelBindingMissingValue);
    public static StronglyTypedIdException ModelBindingInvalidFormat() => new(ErrorCode.ModelBindingInvalidFormat);
    public static StronglyTypedIdException ModelBindingCreationFailure() => new(ErrorCode.ModelBindingCreationFailure);
    public static StronglyTypedIdException InvalidComparison() => new(ErrorCode.InvalidComparison);
    public static StronglyTypedIdException NullId() => new(ErrorCode.NullId);
}