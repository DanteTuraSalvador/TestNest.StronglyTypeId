using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.StronglyTypeIds;

public sealed record GuestId(Guid Value) : StronglyTypedId<GuestId>(Value)
{
    public GuestId() : this(Guid.NewGuid()) { }

    public static GuestId Create(Guid value) => value == Guid.Empty ? throw StronglyTypeIdException.NullId() : new GuestId(value);

    public override string ToString() => Value.ToString();
}
