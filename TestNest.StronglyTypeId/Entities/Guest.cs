using TestNest.StronglyTypeId.StronglyTypeIds;
using TestNest.StronglyTypeId.ValueObjects;

namespace TestNest.StronglyTypeId.Entities;

public sealed class Guest
{
    public GuestId Id { get; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Address Address { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    private Guest(
        GuestId id,
        string firstName,
        string lastName,
        Email email,
        PhoneNumber phoneNumber,
        Address address)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        CreatedAt = DateTime.UtcNow;
    }

    public static Guest Create(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber phoneNumber,
        Address address)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        if (email.IsEmpty())
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return new Guest(
            GuestId.New(),
            firstName.Trim(),
            lastName.Trim(),
            email,
            phoneNumber,
            address);
    }

    public static Guest Create(
        GuestId id,
        string firstName,
        string lastName,
        Email email,
        PhoneNumber phoneNumber,
        Address address)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));

        if (email.IsEmpty())
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return new Guest(
            id,
            firstName.Trim(),
            lastName.Trim(),
            email,
            phoneNumber,
            address);
    }

    public string FullName => $"{FirstName} {LastName}";

    public Guest UpdateFirstName(string newFirstName)
    {
        ValidateName(newFirstName, nameof(newFirstName));
        FirstName = newFirstName.Trim();
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Guest UpdateLastName(string newLastName)
    {
        ValidateName(newLastName, nameof(newLastName));
        LastName = newLastName.Trim();
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Guest UpdateEmail(Email newEmail)
    {
        if (newEmail.IsEmpty())
            throw new ArgumentException("Email cannot be empty", nameof(newEmail));

        Email = newEmail;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Guest UpdatePhoneNumber(PhoneNumber newPhoneNumber)
    {
        PhoneNumber = newPhoneNumber;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Guest UpdateAddress(Address newAddress)
    {
        Address = newAddress;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public bool HasValidContactInfo()
        => !Email.IsEmpty() && !PhoneNumber.IsEmpty();

    public bool HasCompleteProfile()
        => HasValidContactInfo() && !Address.IsEmpty();

    public override string ToString()
        => $"Guest: {FullName} ({Id})";

    private static void ValidateName(string? name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", paramName);

        if (name.Trim().Length < 2)
            throw new ArgumentException("Name must be at least 2 characters", paramName);
    }
}
