namespace TestNest.StronglyTypeId.Exceptions;

public sealed class StronglyTypedIdException : Exception
{
    public enum ErrorCode
    {
        NullInstanceCreation,
        InvalidGuidCreation,
        InvalidFormat,
        NullId
    }

    private static readonly Dictionary<ErrorCode, string> ErrorMessages = new()
    {
        { ErrorCode.NullInstanceCreation, "Failed to create instance of {0}. Activator returned null." },
        { ErrorCode.InvalidGuidCreation, "Invalid GUID provided for {0}." },
        { ErrorCode.InvalidFormat, "Invalid format: {0}." },
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

    public static StronglyTypedIdException NullId() =>
        new(ErrorCode.NullId, ErrorMessages[ErrorCode.NullId]);
}