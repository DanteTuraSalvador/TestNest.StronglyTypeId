using System.Text.RegularExpressions;
using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.ValueObjects;

public sealed partial class Email : ValueObject
{
    private static readonly Lazy<Email> _lazyEmpty = new(() => new Email());
    public static Email Empty => _lazyEmpty.Value;

    public string Address { get; }
    public string LocalPart { get; }
    public string Domain { get; }

    private Email()
    {
        Address = string.Empty;
        LocalPart = string.Empty;
        Domain = string.Empty;
    }

    private Email(string address, string localPart, string domain)
    {
        Address = address;
        LocalPart = localPart;
        Domain = domain;
    }

    public static Email Create(string address)
    {
        ValidateEmail(address);
        var normalized = address.Trim().ToLowerInvariant();
        var parts = normalized.Split('@');
        return new Email(normalized, parts[0], parts[1]);
    }

    public static bool TryCreate(string? address, out Email? email)
    {
        email = null;

        if (string.IsNullOrWhiteSpace(address))
            return false;

        var normalized = address.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
            return false;

        var parts = normalized.Split('@');
        if (parts.Length != 2)
            return false;

        email = new Email(normalized, parts[0], parts[1]);
        return true;
    }

    public static Email Parse(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return Empty;

        return Create(address);
    }

    public static bool TryParse(string? address, out Email email)
    {
        email = Empty;

        if (TryCreate(address, out var result) && result is not null)
        {
            email = result;
            return true;
        }

        return false;
    }

    public bool IsEmpty() => this == Empty;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Address;
    }

    public override string ToString() => IsEmpty() ? "[Empty Email]" : Address;

    private static void ValidateEmail(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw EmailException.EmptyEmail();

        var normalized = address.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
            throw EmailException.InvalidFormat(address);

        var parts = normalized.Split('@');
        if (parts.Length != 2)
            throw EmailException.InvalidFormat(address);

        if (string.IsNullOrWhiteSpace(parts[0]))
            throw EmailException.InvalidLocalPart(address);

        if (string.IsNullOrWhiteSpace(parts[1]) || !parts[1].Contains('.'))
            throw EmailException.InvalidDomain(address);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
