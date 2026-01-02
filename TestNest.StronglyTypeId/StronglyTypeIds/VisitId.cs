using System.Diagnostics.CodeAnalysis;
using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.StronglyTypeIds
{
    public sealed record VisitId : StronglyTypedId<VisitId>
    {
        private static readonly Lazy<VisitId> _lazyEmpty = new(() => new VisitId(Guid.Empty, true), LazyThreadSafetyMode.ExecutionAndPublication);
        public static new VisitId Empty() => _lazyEmpty.Value;

        public VisitId(Guid value) : base(value)
        {
            if (value == Guid.Empty)
                throw StronglyTypedIdException.InvalidGuidCreation(typeof(VisitId));
        }

        public VisitId() : base() { }

        // Private constructor for creating Empty instance
        private VisitId(Guid value, bool _) : base(value, true) { }

        public static VisitId Create(Guid value) => new(value);

        public static new VisitId New() => new(Guid.NewGuid());

        public static VisitId Parse(string input)
        {
            if (!Guid.TryParse(input, out var guid) || guid == Guid.Empty)
                throw StronglyTypedIdException.InvalidFormat(input);

            return new(guid);
        }

        public static explicit operator VisitId(string? input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            return Parse(input);
        }

        public static new bool TryParse(
            [NotNullWhen(true)] string? input,
            [NotNullWhen(true)] out VisitId? result)
        {
            result = null;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out var guid) || guid == Guid.Empty)
                return false;

            result = new VisitId(guid);
            return true;
        }

        public override string ToString() => Value.ToString();
    }
}
