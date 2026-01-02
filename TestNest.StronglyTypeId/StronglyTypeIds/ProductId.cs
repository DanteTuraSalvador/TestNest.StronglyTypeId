using System.Diagnostics.CodeAnalysis;
using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.StronglyTypeIds
{
    public sealed record ProductId : StronglyTypedId<ProductId>
    {

        private static readonly Lazy<ProductId> _lazyEmpty = new(() => new ProductId(Guid.Empty, true), LazyThreadSafetyMode.ExecutionAndPublication);
        public static new ProductId Empty() => _lazyEmpty.Value;


        public ProductId(Guid value) : base(value)
        {
            if (value == Guid.Empty)
                throw StronglyTypedIdException.InvalidGuidCreation(typeof(ProductId));
        }

        public ProductId() : base() { }

        // Private constructor for creating Empty instance
        private ProductId(Guid value, bool _) : base(value, true) { }

        public static ProductId Create(Guid value) => new(value);

        public static new ProductId New() => new(Guid.NewGuid());

        public static ProductId Parse(string input)
        {
            if (!Guid.TryParse(input, out var guid) || guid == Guid.Empty)
                throw StronglyTypedIdException.InvalidFormat(input);

            return new(guid);
        }

        public static explicit operator ProductId(string? input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return Parse(input);
        }

        public static new bool TryParse(
            [NotNullWhen(true)] string? input,
            [NotNullWhen(true)] out ProductId? result)
        {
            result = null;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out var guid) || guid == Guid.Empty)
                return false;

            result = new ProductId(guid);
            return true;
        }

        public override string ToString() => Value.ToString();
    }
}
