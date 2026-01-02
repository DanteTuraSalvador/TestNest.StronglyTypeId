using TestNest.StronglyTypeId.StronglyTypeIds;
using TestNest.StronglyTypeId.ValueObjects;

namespace TestNest.StronglyTypeId.Entities;

public sealed class Customer
{
    public CustomerId Id { get; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Address Address { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    private Customer(
        CustomerId id,
        string name,
        Email email,
        PhoneNumber phoneNumber,
        Address address)
    {
        Id = id;
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        CreatedAt = DateTime.UtcNow;
    }

    public static Customer Create(
        string name,
        Email email,
        PhoneNumber phoneNumber,
        Address address)
    {
        ValidateName(name);

        if (email.IsEmpty())
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return new Customer(
            CustomerId.New(),
            name.Trim(),
            email,
            phoneNumber,
            address);
    }

    public static Customer Create(
        CustomerId id,
        string name,
        Email email,
        PhoneNumber phoneNumber,
        Address address)
    {
        ValidateName(name);

        if (email.IsEmpty())
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return new Customer(
            id,
            name.Trim(),
            email,
            phoneNumber,
            address);
    }

    public Customer UpdateName(string newName)
    {
        ValidateName(newName);
        Name = newName.Trim();
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Customer UpdateEmail(Email newEmail)
    {
        if (newEmail.IsEmpty())
            throw new ArgumentException("Email cannot be empty", nameof(newEmail));

        Email = newEmail;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Customer UpdatePhoneNumber(PhoneNumber newPhoneNumber)
    {
        PhoneNumber = newPhoneNumber;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Customer UpdateAddress(Address newAddress)
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
        => $"Customer: {Name} ({Id})";

    private static void ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        if (name.Trim().Length < 2)
            throw new ArgumentException("Name must be at least 2 characters", nameof(name));
    }
}
