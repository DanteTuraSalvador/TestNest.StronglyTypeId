namespace TestNest.StronglyTypeId.Exceptions;

public sealed class AddressException : Exception
{
    public enum ErrorCode
    {
        EmptyStreet,
        EmptyCity,
        EmptyCountry,
        InvalidPostalCode,
        InvalidAddress
    }

    private static readonly IReadOnlyDictionary<ErrorCode, string> ErrorMessages = new Dictionary<ErrorCode, string>
    {
        [ErrorCode.EmptyStreet] = "Street address cannot be null or empty",
        [ErrorCode.EmptyCity] = "City cannot be null or empty",
        [ErrorCode.EmptyCountry] = "Country cannot be null or empty",
        [ErrorCode.InvalidPostalCode] = "The postal code '{0}' is invalid",
        [ErrorCode.InvalidAddress] = "The address is invalid: {0}"
    };

    public ErrorCode Code { get; }

    private AddressException(ErrorCode code, string message)
        : base(message)
    {
        Code = code;
    }

    public static AddressException EmptyStreet()
        => new(ErrorCode.EmptyStreet, ErrorMessages[ErrorCode.EmptyStreet]);

    public static AddressException EmptyCity()
        => new(ErrorCode.EmptyCity, ErrorMessages[ErrorCode.EmptyCity]);

    public static AddressException EmptyCountry()
        => new(ErrorCode.EmptyCountry, ErrorMessages[ErrorCode.EmptyCountry]);

    public static AddressException InvalidPostalCode(string? postalCode)
        => new(ErrorCode.InvalidPostalCode, string.Format(ErrorMessages[ErrorCode.InvalidPostalCode], postalCode ?? "null"));

    public static AddressException InvalidAddress(string reason)
        => new(ErrorCode.InvalidAddress, string.Format(ErrorMessages[ErrorCode.InvalidAddress], reason));
}
