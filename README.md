# 🚀 StronglyTypedId in C#

`StronglyTypedId<T>` is a base class for creating strongly typed IDs in C#. It encapsulates `Guid` values, ensuring type safety and improving code readability. This is useful for domain-driven design (DDD) to distinguish different entity IDs.

## ✨ Features

- ✅ **Strong Typing**: Avoids confusion between different entity IDs.
- ✅ **Immutable**: Ensures IDs cannot be modified after creation.
- ✅ **Automatic ID Generation**: Supports `Guid.NewGuid()`.
- ✅ **Parsing and Validation**: Methods to parse from `string` and `Guid`.
- ✅ **Comparison and Equality**: Implements `IComparable` and `IEquatable`.

---

## 📌 Implementation

### 🔹 StronglyTypedId Base Class

```csharp
using System;
using System.Diagnostics.CodeAnalysis;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.Common
{
    public abstract record StronglyTypedId<T> : IComparable<T>, IEquatable<T>, IComparable
        where T : StronglyTypedId<T>
    {
        private static readonly Lazy<T> _lazyEmpty = new(() =>
        {
            var instance = Activator.CreateInstance(typeof(T), Guid.Empty, true) as T;
            return instance ?? throw StronglyTypedIdException.NullInstanceCreation(typeof(T));
        });

        public Guid Value { get; }

        protected StronglyTypedId() => Value = Guid.NewGuid();

        protected StronglyTypedId(Guid value, bool _ = false) => Value = value;

        public static T Empty() => _lazyEmpty.Value;

        public static T New() => Activator.CreateInstance(typeof(T), Guid.NewGuid()) as T
            ?? throw StronglyTypedIdException.NullInstanceCreation(typeof(T));

        public static implicit operator Guid(StronglyTypedId<T> id) => id.Value;

        public static implicit operator StronglyTypedId<T>?(string? input) =>
            TryParse(input, out var id) ? id : null;

        public static bool TryParse(string? input, out T? result)
        {
            result = null;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out var guid))
                return false;

            result = Activator.CreateInstance(typeof(T), guid) as T;
            return result is not null;
        }

        public static bool TryParse(Guid input, out T? result)
        {
            result = null;
            if (input == Guid.Empty)
                return false;

            result = Activator.CreateInstance(typeof(T), input) as T;
            return result is not null;
        }

        public bool Equals(T? other) => other is not null && Value.Equals(other.Value);

        public override int GetHashCode() => Value.GetHashCode();

        public int CompareTo(T? other) => other is null ? 1 : Value.CompareTo(other.Value);

        int IComparable.CompareTo(object? obj)
        {
            if (obj is null) return 1;
            if (obj is not T other)
                throw new ArgumentException($"Object must be of type {typeof(T).Name}");
            return CompareTo(other);
        }

        public override string ToString() => Value.ToString();
    }
}

### 🔹 StronglyTypedId Base Class

## 🔹 Example: Creating a `GuestId` Strongly Typed ID

```csharp
using System.Diagnostics.CodeAnalysis;
using System.Threading;
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
