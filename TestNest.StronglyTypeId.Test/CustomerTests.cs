using TestNest.StronglyTypeId.Entities;
using TestNest.StronglyTypeId.StronglyTypeIds;
using TestNest.StronglyTypeId.ValueObjects;

namespace TestNest.StronglyTypeId.Test;

public class CustomerTests
{
    private static Email CreateValidEmail() => Email.Create("test@example.com");
    private static PhoneNumber CreateValidPhone() => PhoneNumber.Create("+1", "5551234567");
    private static Address CreateValidAddress() => Address.Create("123 Main St", "New York", "NY", "10001", "USA");

    [Fact]
    public void Create_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var name = "John Doe";
        var email = CreateValidEmail();
        var phone = CreateValidPhone();
        var address = CreateValidAddress();

        // Act
        var customer = Customer.Create(name, email, phone, address);

        // Assert
        Assert.NotNull(customer);
        Assert.NotEqual(CustomerId.Empty(), customer.Id);
        Assert.Equal(name, customer.Name);
        Assert.Equal(email, customer.Email);
        Assert.Equal(phone, customer.PhoneNumber);
        Assert.Equal(address, customer.Address);
        Assert.Null(customer.UpdatedAt);
    }

    [Fact]
    public void Create_WithExistingId_ShouldUseProvidedId()
    {
        // Arrange
        var id = CustomerId.New();
        var name = "John Doe";
        var email = CreateValidEmail();
        var phone = CreateValidPhone();
        var address = CreateValidAddress();

        // Act
        var customer = Customer.Create(id, name, email, phone, address);

        // Assert
        Assert.Equal(id, customer.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? invalidName)
    {
        // Arrange
        var email = CreateValidEmail();
        var phone = CreateValidPhone();
        var address = CreateValidAddress();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Customer.Create(invalidName!, email, phone, address));
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var name = "John Doe";
        var email = Email.Empty;
        var phone = CreateValidPhone();
        var address = CreateValidAddress();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Customer.Create(name, email, phone, address));
    }

    [Fact]
    public void Create_WithEmptyPhoneNumber_ShouldSucceed()
    {
        // Arrange
        var name = "John Doe";
        var email = CreateValidEmail();
        var phone = PhoneNumber.Empty;
        var address = CreateValidAddress();

        // Act
        var customer = Customer.Create(name, email, phone, address);

        // Assert
        Assert.Equal(PhoneNumber.Empty, customer.PhoneNumber);
    }

    [Fact]
    public void Create_WithEmptyAddress_ShouldSucceed()
    {
        // Arrange
        var name = "John Doe";
        var email = CreateValidEmail();
        var phone = CreateValidPhone();
        var address = Address.Empty;

        // Act
        var customer = Customer.Create(name, email, phone, address);

        // Assert
        Assert.Equal(Address.Empty, customer.Address);
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var customer = Customer.Create("John Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());
        var newName = "Jane Doe";

        // Act
        customer.UpdateName(newName);

        // Assert
        Assert.Equal(newName, customer.Name);
        Assert.NotNull(customer.UpdatedAt);
    }

    [Fact]
    public void UpdateEmail_WithValidEmail_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var customer = Customer.Create("John Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());
        var newEmail = Email.Create("newemail@example.com");

        // Act
        customer.UpdateEmail(newEmail);

        // Assert
        Assert.Equal(newEmail, customer.Email);
        Assert.NotNull(customer.UpdatedAt);
    }

    [Fact]
    public void UpdateEmail_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var customer = Customer.Create("John Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => customer.UpdateEmail(Email.Empty));
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var customer = Customer.Create("John Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());
        var newPhone = PhoneNumber.Create("+44", "7911123456");

        // Act
        customer.UpdatePhoneNumber(newPhone);

        // Assert
        Assert.Equal(newPhone, customer.PhoneNumber);
        Assert.NotNull(customer.UpdatedAt);
    }

    [Fact]
    public void UpdateAddress_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var customer = Customer.Create("John Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());
        var newAddress = Address.Create("456 Oak Ave", "Los Angeles", "CA", "90001", "USA");

        // Act
        customer.UpdateAddress(newAddress);

        // Assert
        Assert.Equal(newAddress, customer.Address);
        Assert.NotNull(customer.UpdatedAt);
    }

    [Fact]
    public void HasValidContactInfo_WithEmailAndPhone_ShouldReturnTrue()
    {
        // Arrange
        var customer = Customer.Create("John Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());

        // Act & Assert
        Assert.True(customer.HasValidContactInfo());
    }

    [Fact]
    public void HasValidContactInfo_WithEmptyPhone_ShouldReturnFalse()
    {
        // Arrange
        var customer = Customer.Create("John Doe", CreateValidEmail(), PhoneNumber.Empty, CreateValidAddress());

        // Act & Assert
        Assert.False(customer.HasValidContactInfo());
    }

    [Fact]
    public void HasCompleteProfile_WithAllData_ShouldReturnTrue()
    {
        // Arrange
        var customer = Customer.Create("John Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());

        // Act & Assert
        Assert.True(customer.HasCompleteProfile());
    }

    [Fact]
    public void HasCompleteProfile_WithEmptyAddress_ShouldReturnFalse()
    {
        // Arrange
        var customer = Customer.Create("John Doe", CreateValidEmail(), CreateValidPhone(), Address.Empty);

        // Act & Assert
        Assert.False(customer.HasCompleteProfile());
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var customer = Customer.Create("John Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());

        // Act
        var result = customer.ToString();

        // Assert
        Assert.Contains("Customer:", result);
        Assert.Contains("John Doe", result);
    }
}
