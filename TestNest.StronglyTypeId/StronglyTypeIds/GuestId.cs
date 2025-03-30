// GuestId.cs
using System.Diagnostics.CodeAnalysis;
using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.StronglyTypeIds;

public sealed record GuestId : StronglyTypedId<GuestId>
{
    public GuestId(Guid value) : base(value)
    {
        if (value == Guid.Empty)
            throw StronglyTypedIdException.InvalidGuidCreation(typeof(GuestId));
    }

    public GuestId() : this(Guid.NewGuid()) { }

    public static GuestId Create(Guid value) => new(value);

    public static GuestId Parse(string input)
    {
        if (!Guid.TryParse(input, out var guid) || guid == Guid.Empty)
            throw StronglyTypedIdException.InvalidFormat(input);

        return new(guid);
    }

    public static explicit operator GuestId(string? input)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));

        return Parse(input);
    }

    public static new bool TryParse(
        [NotNullWhen(true)] string? input,
        [NotNullWhen(true)] out GuestId? result)
    {
        result = null;
        if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out var guid) || guid == Guid.Empty)
            return false;

        result = new GuestId(guid);
        return true;
    }

    public static GuestId New() => new(Guid.NewGuid());
    private GuestId(Guid value, bool allowEmpty) : base(value) { }

    public static GuestId Empty { get; } = new(Guid.Empty, true);
    public override string ToString() => Value.ToString();
}