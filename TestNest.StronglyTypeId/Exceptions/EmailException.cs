namespace TestNest.StronglyTypeId.Exceptions;

public sealed class EmailException : Exception
{
    public enum ErrorCode
    {
        EmptyEmail,
        InvalidFormat,
        InvalidLocalPart,
        InvalidDomain
    }

    private static readonly IReadOnlyDictionary<ErrorCode, string> ErrorMessages = new Dictionary<ErrorCode, string>
    {
        [ErrorCode.EmptyEmail] = "Email address cannot be null or empty",
        [ErrorCode.InvalidFormat] = "The email address '{0}' has an invalid format",
        [ErrorCode.InvalidLocalPart] = "The local part of email '{0}' is invalid",
        [ErrorCode.InvalidDomain] = "The domain part of email '{0}' is invalid"
    };

    public ErrorCode Code { get; }

    private EmailException(ErrorCode code, string message)
        : base(message)
    {
        Code = code;
    }

    public static EmailException EmptyEmail()
        => new(ErrorCode.EmptyEmail, ErrorMessages[ErrorCode.EmptyEmail]);

    public static EmailException InvalidFormat(string? email)
        => new(ErrorCode.InvalidFormat, string.Format(ErrorMessages[ErrorCode.InvalidFormat], email ?? "null"));

    public static EmailException InvalidLocalPart(string? email)
        => new(ErrorCode.InvalidLocalPart, string.Format(ErrorMessages[ErrorCode.InvalidLocalPart], email ?? "null"));

    public static EmailException InvalidDomain(string? email)
        => new(ErrorCode.InvalidDomain, string.Format(ErrorMessages[ErrorCode.InvalidDomain], email ?? "null"));
}
