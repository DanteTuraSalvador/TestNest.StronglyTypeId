using System.Diagnostics.CodeAnalysis;
using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.StronglyTypeIds
{
    public sealed record GuestId : StronglyTypedId<GuestId>
    {

        private static readonly Lazy<GuestId> _lazyEmpty = new(() => new GuestId(Guid.Empty, true), LazyThreadSafetyMode.ExecutionAndPublication);
        public static new GuestId Empty() => _lazyEmpty.Value;


        public GuestId(Guid value) : base(value)
        {
            if (value == Guid.Empty)
                throw StronglyTypedIdException.InvalidGuidCreation(typeof(GuestId));
        }

        public GuestId() : base() { }

        // Protected constructor for base class to create Empty instance
        protected GuestId(Guid value, bool _) : base(value, true) { }

        public static GuestId Create(Guid value) => new(value);

        public static new GuestId New() => new(Guid.NewGuid());

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

        public override string ToString() => Value.ToString();
    }
}