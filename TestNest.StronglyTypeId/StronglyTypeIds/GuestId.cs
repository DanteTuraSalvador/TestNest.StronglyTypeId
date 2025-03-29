using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.StronglyTypeIds;

// This record represents a unique ID for a guest, wrapping a Guid.
public sealed record GuestId(Guid Value) : StronglyTypedId<GuestId>(Value)
{
    // Default constructor: If no value is provided, generate a new unique GUID.
    public GuestId() : this(Guid.NewGuid()) { }

    // Factory method to create a GuestId from an existing GUID.
    public static GuestId Create(Guid value) =>
        // If the given GUID is empty, throw a custom exception.
        value == Guid.Empty
            ? throw StronglyTypedIdException.NullId()
            : new GuestId(value);

    // Returns the string representation of the GUID, making it easier to print or log.
    public override string ToString() => Value.ToString();
}
