﻿// StronglyTypedId.cs
using System;
using System.Diagnostics.CodeAnalysis;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.Common;

public abstract record StronglyTypedId<T> : IComparable<T>, IEquatable<T>, IComparable
    where T : StronglyTypedId<T>
{
    public Guid Value { get; }

    protected StronglyTypedId() => Value = Guid.NewGuid();

    public StronglyTypedId(Guid value)
    {
        Value = value == Guid.Empty ? Guid.Empty : value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(StronglyTypedId<T> id) => id.Value;

    public static T Empty()
    {
        var idInstance = Activator.CreateInstance(typeof(T), Guid.Empty) as T;
        return idInstance ?? throw StronglyTypedIdException.NullInstanceCreation(typeof(T));
    }

    public static implicit operator StronglyTypedId<T>?(string? input) =>
        TryParse(input, out var id) ? id : null;

    public static T New() => Activator.CreateInstance(typeof(T), Guid.NewGuid()) as T
        ?? throw StronglyTypedIdException.NullInstanceCreation(typeof(T));

    public static bool TryParse(string? input, out T? result)
    {
        result = null;

        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        if (!Guid.TryParse(input, out var guid))
        {
            return false;
        }

        result = Activator.CreateInstance(typeof(T), guid) as T;
        return result is not null;
    }

    public static bool TryParse(Guid input, out T? result)
    {
        if (input == Guid.Empty)
        {
            result = null;
            return false;
        }

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
}