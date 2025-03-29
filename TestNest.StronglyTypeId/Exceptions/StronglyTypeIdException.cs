namespace TestNest.StronglyTypeId.Exceptions;
public sealed class StronglyTypeIdException : Exception
{
    public enum ErrorCode
    {
        NullId
    }

    private static readonly Dictionary<ErrorCode, string> ErrorMessages = new()
    {
        { ErrorCode.NullId, "Id cannot be null or empty" }
    };

    public ErrorCode Code { get; }

    private StronglyTypeIdException(ErrorCode code) : base(ErrorMessages[code])
    {
        Code = code;
    }

    public static StronglyTypeIdException NullId() =>
        new(ErrorCode.NullId);


}