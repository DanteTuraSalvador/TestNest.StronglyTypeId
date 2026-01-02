using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.ValueObjects;

public sealed class Address : ValueObject
{
    private static readonly Lazy<Address> _lazyEmpty = new(() => new Address());
    public static Address Empty => _lazyEmpty.Value;

    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address()
    {
        Street = string.Empty;
        City = string.Empty;
        State = string.Empty;
        PostalCode = string.Empty;
        Country = string.Empty;
    }

    private Address(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Address Create(string street, string city, string state, string postalCode, string country)
    {
        ValidateStreet(street);
        ValidateCity(city);
        ValidateCountry(country);

        return new Address(
            street.Trim(),
            city.Trim(),
            state?.Trim() ?? string.Empty,
            postalCode?.Trim() ?? string.Empty,
            country.Trim()
        );
    }

    public static Address Create(string street, string city, string country)
        => Create(street, city, string.Empty, string.Empty, country);

    public static bool TryCreate(
        string? street,
        string? city,
        string? state,
        string? postalCode,
        string? country,
        out Address? address)
    {
        address = null;

        if (string.IsNullOrWhiteSpace(street))
            return false;

        if (string.IsNullOrWhiteSpace(city))
            return false;

        if (string.IsNullOrWhiteSpace(country))
            return false;

        address = new Address(
            street.Trim(),
            city.Trim(),
            state?.Trim() ?? string.Empty,
            postalCode?.Trim() ?? string.Empty,
            country.Trim()
        );

        return true;
    }

    public Address WithStreet(string newStreet)
        => Create(newStreet, City, State, PostalCode, Country);

    public Address WithCity(string newCity)
        => Create(Street, newCity, State, PostalCode, Country);

    public Address WithState(string newState)
        => Create(Street, City, newState, PostalCode, Country);

    public Address WithPostalCode(string newPostalCode)
        => Create(Street, City, State, newPostalCode, Country);

    public Address WithCountry(string newCountry)
        => Create(Street, City, State, PostalCode, newCountry);

    public bool IsEmpty() => this == Empty;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString()
    {
        if (IsEmpty()) return "[Empty Address]";
        return ToSingleLineString();
    }

    public string ToSingleLineString()
    {
        if (IsEmpty()) return "[Empty Address]";

        var parts = new List<string> { Street, City };

        if (!string.IsNullOrWhiteSpace(State))
            parts.Add(State);

        if (!string.IsNullOrWhiteSpace(PostalCode))
            parts.Add(PostalCode);

        parts.Add(Country);

        return string.Join(", ", parts);
    }

    public string ToMultiLineString()
    {
        if (IsEmpty()) return "[Empty Address]";

        var lines = new List<string> { Street };

        var cityLine = City;
        if (!string.IsNullOrWhiteSpace(State))
            cityLine += $", {State}";
        if (!string.IsNullOrWhiteSpace(PostalCode))
            cityLine += $" {PostalCode}";
        lines.Add(cityLine);

        lines.Add(Country);

        return string.Join(Environment.NewLine, lines);
    }

    private static void ValidateStreet(string? street)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw AddressException.EmptyStreet();
    }

    private static void ValidateCity(string? city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw AddressException.EmptyCity();
    }

    private static void ValidateCountry(string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
            throw AddressException.EmptyCountry();
    }
}
