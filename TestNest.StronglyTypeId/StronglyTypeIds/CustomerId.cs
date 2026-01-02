using System.Diagnostics.CodeAnalysis;
using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.StronglyTypeIds
{
    public sealed record CustomerId : StronglyTypedId<CustomerId>
    {

        private static readonly Lazy<CustomerId> _lazyEmpty = new(() => new CustomerId(Guid.Empty, true), LazyThreadSafetyMode.ExecutionAndPublication);
        public static new CustomerId Empty() => _lazyEmpty.Value;


        public CustomerId(Guid value) : base(value)
        {
            if (value == Guid.Empty)
                throw StronglyTypedIdException.InvalidGuidCreation(typeof(CustomerId));
        }

        public CustomerId() : base() { }

        // Private constructor for creating Empty instance
        private CustomerId(Guid value, bool _) : base(value, true) { }

        public static CustomerId Create(Guid value) => new(value);

        public static new CustomerId New() => new(Guid.NewGuid());

        public static CustomerId Parse(string input)
        {
            if (!Guid.TryParse(input, out var guid) || guid == Guid.Empty)
                throw StronglyTypedIdException.InvalidFormat(input);

            return new(guid);
        }

        public static explicit operator CustomerId(string? input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return Parse(input);
        }

        public static new bool TryParse(
            [NotNullWhen(true)] string? input,
            [NotNullWhen(true)] out CustomerId? result)
        {
            result = null;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out var guid) || guid == Guid.Empty)
                return false;

            result = new CustomerId(guid);
            return true;
        }

        public override string ToString() => Value.ToString();
    }
}
