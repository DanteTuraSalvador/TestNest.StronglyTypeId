using System.Text.RegularExpressions;
using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.ValueObjects;

public sealed partial class PhoneNumber : ValueObject
{
    private static readonly HashSet<string> ValidCountryCodes = new()
    {
        "+1", "+44", "+63", "+81", "+86", "+91", "+49", "+33", "+39", "+34"
    };

    private static readonly Lazy<PhoneNumber> _lazyEmpty = new(() => new PhoneNumber());
    public static PhoneNumber Empty => _lazyEmpty.Value;

    public string CountryCode { get; }
    public string Number { get; }
    public string FullNumber { get; }

    private PhoneNumber()
    {
        CountryCode = string.Empty;
        Number = string.Empty;
        FullNumber = string.Empty;
    }

    private PhoneNumber(string countryCode, string number)
    {
        CountryCode = countryCode;
        Number = number;
        FullNumber = $"{countryCode}{number}";
    }

    public static PhoneNumber Create(string countryCode, string number)
    {
        ValidateCountryCode(countryCode);
        ValidateNumber(number);
        var normalizedNumber = NormalizeNumber(number);
        return new PhoneNumber(countryCode, normalizedNumber);
    }

    public static PhoneNumber Create(string fullNumber)
    {
        if (string.IsNullOrWhiteSpace(fullNumber))
            throw PhoneNumberException.EmptyPhoneNumber();

        var normalized = NormalizeNumber(fullNumber);

        if (!normalized.StartsWith('+'))
            throw PhoneNumberException.InvalidFormat(fullNumber);

        var (countryCode, number) = ParseFullNumber(normalized);
        ValidateCountryCode(countryCode);
        ValidateNumber(number);

        return new PhoneNumber(countryCode, number);
    }

    public static bool TryCreate(string countryCode, string number, out PhoneNumber? phoneNumber)
    {
        phoneNumber = null;

        if (string.IsNullOrWhiteSpace(countryCode) || !ValidCountryCodes.Contains(countryCode))
            return false;

        if (string.IsNullOrWhiteSpace(number))
            return false;

        var normalizedNumber = NormalizeNumber(number);
        if (normalizedNumber.Length < 7 || normalizedNumber.Length > 15)
            return false;

        if (!DigitsOnlyRegex().IsMatch(normalizedNumber))
            return false;

        phoneNumber = new PhoneNumber(countryCode, normalizedNumber);
        return true;
    }

    public static bool TryCreate(string fullNumber, out PhoneNumber? phoneNumber)
    {
        phoneNumber = null;

        if (string.IsNullOrWhiteSpace(fullNumber))
            return false;

        var normalized = NormalizeNumber(fullNumber);

        if (!normalized.StartsWith('+'))
            return false;

        var (countryCode, number) = ParseFullNumber(normalized);
        return TryCreate(countryCode, number, out phoneNumber);
    }

    public static PhoneNumber Parse(string fullNumber)
    {
        if (string.IsNullOrWhiteSpace(fullNumber))
            return Empty;

        return Create(fullNumber);
    }

    public static bool TryParse(string? fullNumber, out PhoneNumber phoneNumber)
    {
        phoneNumber = Empty;

        if (string.IsNullOrWhiteSpace(fullNumber))
            return false;

        if (TryCreate(fullNumber, out var result) && result is not null)
        {
            phoneNumber = result;
            return true;
        }

        return false;
    }

    public bool IsEmpty() => this == Empty;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return CountryCode;
        yield return Number;
    }

    public override string ToString() => IsEmpty() ? "[Empty PhoneNumber]" : FullNumber;

    public string ToFormattedString()
    {
        if (IsEmpty()) return "[Empty PhoneNumber]";

        return Number.Length >= 10
            ? $"{CountryCode} ({Number[..3]}) {Number[3..6]}-{Number[6..]}"
            : FullNumber;
    }

    public static IReadOnlyCollection<string> GetValidCountryCodes() =>
        ValidCountryCodes.ToList().AsReadOnly();

    private static void ValidateCountryCode(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw PhoneNumberException.InvalidCountryCode(countryCode);

        if (!ValidCountryCodes.Contains(countryCode))
            throw PhoneNumberException.InvalidCountryCode(countryCode);
    }

    private static void ValidateNumber(string? number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw PhoneNumberException.EmptyPhoneNumber();

        var normalized = NormalizeNumber(number);

        if (normalized.Length < 7 || normalized.Length > 15)
            throw PhoneNumberException.InvalidNumber(number);

        if (!DigitsOnlyRegex().IsMatch(normalized))
            throw PhoneNumberException.InvalidNumber(number);
    }

    private static string NormalizeNumber(string number)
    {
        return new string(number.Where(c => char.IsDigit(c) || c == '+').ToArray());
    }

    private static (string CountryCode, string Number) ParseFullNumber(string fullNumber)
    {
        foreach (var code in ValidCountryCodes.OrderByDescending(c => c.Length))
        {
            if (fullNumber.StartsWith(code))
            {
                return (code, fullNumber[code.Length..]);
            }
        }

        // Default: assume first 2-3 chars are country code
        var countryCodeLength = fullNumber.Length > 2 && char.IsDigit(fullNumber[2]) ? 2 : 3;
        return (fullNumber[..countryCodeLength], fullNumber[countryCodeLength..]);
    }

    [GeneratedRegex(@"^\d+$", RegexOptions.Compiled)]
    private static partial Regex DigitsOnlyRegex();
}
