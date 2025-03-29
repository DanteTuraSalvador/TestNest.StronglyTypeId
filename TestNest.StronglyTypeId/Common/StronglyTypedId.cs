using Microsoft.AspNetCore.Mvc; // Enables ASP.NET Core model binding.
using System.Text.Json.Serialization; // Supports JSON serialization & deserialization.
using TestNest.StronglyTypeId.Exceptions; // Imports custom exceptions for strongly typed IDs.

namespace TestNest.StronglyTypeId.Common;

// This is the base class for all strongly typed ID types.
// It ensures that every ID type behaves like a GUID but with a unique type.
[ModelBinder(typeof(StronglyTypedIdModelBinder<>))] // Tells ASP.NET Core to use a custom model binder.
[JsonConverter(typeof(StronglyTypedIdJsonConverter<>))] // Tells JSON serialization to use our custom converter.
public abstract record StronglyTypedId<T> : IComparable<T>, IEquatable<T>, IComparable
    where T : StronglyTypedId<T> // Ensures that only derived classes can be strongly typed IDs.
{
    // The actual GUID value representing the strongly typed ID.
    public Guid Value { get; }

    // Default constructor: Generates a new GUID.
    protected StronglyTypedId() => Value = Guid.NewGuid();

    // Constructor with a provided GUID. If it's empty, it stays empty.
    public StronglyTypedId(Guid value)
    {
        Value = value == Guid.Empty ? Guid.Empty : value;
    }

    // Converts the strongly typed ID to a string (for debugging, logging, etc.).
    public override string ToString() => Value.ToString();

    // Implicitly converts a strongly typed ID to a GUID.
    public static implicit operator Guid(StronglyTypedId<T> id) => id.Value;

    // Creates an empty instance of the strongly typed ID.
    public static T Empty()
    {
        // Uses reflection to create an instance of the derived type with an empty GUID.
        var idInstance = Activator.CreateInstance(typeof(T), Guid.Empty) as T;

        // If creation fails (which shouldn't happen), throw an exception.
        if (idInstance is null)
            throw StronglyTypedIdException.NullInstance();

        return idInstance;
    }

    // Allows conversion from a string to a strongly typed ID.
    public static implicit operator StronglyTypedId<T>?(string? input) =>
        TryParse(input, out var id) ? id : null;

    // Creates a new instance with a fresh GUID.
    public static T New() => Activator.CreateInstance(typeof(T), Guid.NewGuid()) as T
        ?? throw StronglyTypedIdException.NullInstance();

    // Tries to parse a string into a strongly typed ID.
    public static bool TryParse(string? input, out T? result)
    {
        result = null;

        // Check if the input string is a valid GUID.
        if (Guid.TryParse(input, out var guid))
        {
            // Create an instance of the strongly typed ID using the parsed GUID.
            result = Activator.CreateInstance(typeof(T), guid) as T;
            return result is not null; // Return true if successful.
        }

        return false; // Parsing failed.
    }

    // Tries to parse a GUID into a strongly typed ID.
    public static bool TryParse(Guid input, out T? result)
    {
        // Create an instance using the given GUID.
        result = Activator.CreateInstance(typeof(T), input) as T;
        return result is not null;
    }

    // Checks if this ID is equal to another ID (by comparing the GUID values).
    public bool Equals(T? other) => other is not null && Value.Equals(other.Value);

    // Generates a hash code based on the GUID value.
    public override int GetHashCode() => Value.GetHashCode();

    // Compares this ID to another one (used for sorting, comparisons, etc.).
    public int CompareTo(T? other) => other is null ? 1 : Value.CompareTo(other.Value);

    // Generic comparison method (supports comparing to any object).
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null) return 1; // Null objects are considered "less than" this one.

        // If the object isn't the same type as this ID, throw an exception.
        if (obj is not T) throw StronglyTypedIdException.InvalidComparison();

        // Perform a strongly typed comparison.
        return CompareTo((T)obj);
    }
}
