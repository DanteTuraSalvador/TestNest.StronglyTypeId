using System.Diagnostics.CodeAnalysis;
using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.StronglyTypeIds
{
    public sealed record OrderId : StronglyTypedId<OrderId>
    {

        private static readonly Lazy<OrderId> _lazyEmpty = new(() => new OrderId(Guid.Empty, true), LazyThreadSafetyMode.ExecutionAndPublication);
        public static new OrderId Empty() => _lazyEmpty.Value;


        public OrderId(Guid value) : base(value)
        {
            if (value == Guid.Empty)
                throw StronglyTypedIdException.InvalidGuidCreation(typeof(OrderId));
        }

        public OrderId() : base() { }

        // Private constructor for creating Empty instance
        private OrderId(Guid value, bool _) : base(value, true) { }

        public static OrderId Create(Guid value) => new(value);

        public static new OrderId New() => new(Guid.NewGuid());

        public static OrderId Parse(string input)
        {
            if (!Guid.TryParse(input, out var guid) || guid == Guid.Empty)
                throw StronglyTypedIdException.InvalidFormat(input);

            return new(guid);
        }

        public static explicit operator OrderId(string? input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return Parse(input);
        }

        public static new bool TryParse(
            [NotNullWhen(true)] string? input,
            [NotNullWhen(true)] out OrderId? result)
        {
            result = null;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out var guid) || guid == Guid.Empty)
                return false;

            result = new OrderId(guid);
            return true;
        }

        public override string ToString() => Value.ToString();
    }
}
