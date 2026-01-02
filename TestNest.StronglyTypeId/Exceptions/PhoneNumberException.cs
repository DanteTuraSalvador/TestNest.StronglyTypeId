namespace TestNest.StronglyTypeId.Exceptions;

public sealed class PhoneNumberException : Exception
{
    public enum ErrorCode
    {
        EmptyPhoneNumber,
        InvalidFormat,
        InvalidCountryCode,
        InvalidNumber
    }

    private static readonly IReadOnlyDictionary<ErrorCode, string> ErrorMessages = new Dictionary<ErrorCode, string>
    {
        [ErrorCode.EmptyPhoneNumber] = "Phone number cannot be null or empty",
        [ErrorCode.InvalidFormat] = "The phone number '{0}' has an invalid format",
        [ErrorCode.InvalidCountryCode] = "The country code '{0}' is invalid",
        [ErrorCode.InvalidNumber] = "The phone number '{0}' is invalid"
    };

    public ErrorCode Code { get; }

    private PhoneNumberException(ErrorCode code, string message)
        : base(message)
    {
        Code = code;
    }

    public static PhoneNumberException EmptyPhoneNumber()
        => new(ErrorCode.EmptyPhoneNumber, ErrorMessages[ErrorCode.EmptyPhoneNumber]);

    public static PhoneNumberException InvalidFormat(string? phoneNumber)
        => new(ErrorCode.InvalidFormat, string.Format(ErrorMessages[ErrorCode.InvalidFormat], phoneNumber ?? "null"));

    public static PhoneNumberException InvalidCountryCode(string? countryCode)
        => new(ErrorCode.InvalidCountryCode, string.Format(ErrorMessages[ErrorCode.InvalidCountryCode], countryCode ?? "null"));

    public static PhoneNumberException InvalidNumber(string? number)
        => new(ErrorCode.InvalidNumber, string.Format(ErrorMessages[ErrorCode.InvalidNumber], number ?? "null"));
}
