using TestNest.StronglyTypeId.Entities;
using TestNest.StronglyTypeId.StronglyTypeIds;
using TestNest.StronglyTypeId.ValueObjects;

namespace TestNest.StronglyTypeId.Test;

public class GuestEntityTests
{
    private static Email CreateValidEmail() => Email.Create("guest@example.com");
    private static PhoneNumber CreateValidPhone() => PhoneNumber.Create("+1", "5551234567");
    private static Address CreateValidAddress() => Address.Create("123 Main St", "New York", "NY", "10001", "USA");

    [Fact]
    public void Create_WithValidData_ShouldCreateGuest()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = CreateValidEmail();
        var phone = CreateValidPhone();
        var address = CreateValidAddress();

        // Act
        var guest = Guest.Create(firstName, lastName, email, phone, address);

        // Assert
        Assert.NotNull(guest);
        Assert.NotEqual(GuestId.Empty(), guest.Id);
        Assert.Equal(firstName, guest.FirstName);
        Assert.Equal(lastName, guest.LastName);
        Assert.Equal(email, guest.Email);
        Assert.Equal(phone, guest.PhoneNumber);
        Assert.Equal(address, guest.Address);
        Assert.Null(guest.UpdatedAt);
    }

    [Fact]
    public void Create_WithExistingId_ShouldUseProvidedId()
    {
        // Arrange
        var id = GuestId.New();
        var email = CreateValidEmail();
        var phone = CreateValidPhone();
        var address = CreateValidAddress();

        // Act
        var guest = Guest.Create(id, "John", "Doe", email, phone, address);

        // Assert
        Assert.Equal(id, guest.Id);
    }

    [Theory]
    [InlineData(null, "Doe")]
    [InlineData("", "Doe")]
    [InlineData("   ", "Doe")]
    [InlineData("A", "Doe")]
    [InlineData("John", null)]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    [InlineData("John", "D")]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? firstName, string? lastName)
    {
        // Arrange
        var email = CreateValidEmail();
        var phone = CreateValidPhone();
        var address = CreateValidAddress();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guest.Create(firstName!, lastName!, email, phone, address));
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var email = Email.Empty;
        var phone = CreateValidPhone();
        var address = CreateValidAddress();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guest.Create("John", "Doe", email, phone, address));
    }

    [Fact]
    public void FullName_ShouldReturnCombinedNames()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());

        // Act & Assert
        Assert.Equal("John Doe", guest.FullName);
    }

    [Fact]
    public void UpdateFirstName_WithValidName_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());
        var newFirstName = "Jane";

        // Act
        guest.UpdateFirstName(newFirstName);

        // Assert
        Assert.Equal(newFirstName, guest.FirstName);
        Assert.Equal("Jane Doe", guest.FullName);
        Assert.NotNull(guest.UpdatedAt);
    }

    [Fact]
    public void UpdateLastName_WithValidName_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());
        var newLastName = "Smith";

        // Act
        guest.UpdateLastName(newLastName);

        // Assert
        Assert.Equal(newLastName, guest.LastName);
        Assert.Equal("John Smith", guest.FullName);
        Assert.NotNull(guest.UpdatedAt);
    }

    [Fact]
    public void UpdateEmail_WithValidEmail_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());
        var newEmail = Email.Create("newemail@example.com");

        // Act
        guest.UpdateEmail(newEmail);

        // Assert
        Assert.Equal(newEmail, guest.Email);
        Assert.NotNull(guest.UpdatedAt);
    }

    [Fact]
    public void UpdateEmail_WithEmptyEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => guest.UpdateEmail(Email.Empty));
    }

    [Fact]
    public void UpdatePhoneNumber_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());
        var newPhone = PhoneNumber.Create("+44", "7911123456");

        // Act
        guest.UpdatePhoneNumber(newPhone);

        // Assert
        Assert.Equal(newPhone, guest.PhoneNumber);
        Assert.NotNull(guest.UpdatedAt);
    }

    [Fact]
    public void UpdateAddress_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());
        var newAddress = Address.Create("456 Oak Ave", "Los Angeles", "CA", "90001", "USA");

        // Act
        guest.UpdateAddress(newAddress);

        // Assert
        Assert.Equal(newAddress, guest.Address);
        Assert.NotNull(guest.UpdatedAt);
    }

    [Fact]
    public void HasValidContactInfo_WithEmailAndPhone_ShouldReturnTrue()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());

        // Act & Assert
        Assert.True(guest.HasValidContactInfo());
    }

    [Fact]
    public void HasValidContactInfo_WithEmptyPhone_ShouldReturnFalse()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), PhoneNumber.Empty, CreateValidAddress());

        // Act & Assert
        Assert.False(guest.HasValidContactInfo());
    }

    [Fact]
    public void HasCompleteProfile_WithAllData_ShouldReturnTrue()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());

        // Act & Assert
        Assert.True(guest.HasCompleteProfile());
    }

    [Fact]
    public void HasCompleteProfile_WithEmptyAddress_ShouldReturnFalse()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), CreateValidPhone(), Address.Empty);

        // Act & Assert
        Assert.False(guest.HasCompleteProfile());
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var guest = Guest.Create("John", "Doe", CreateValidEmail(), CreateValidPhone(), CreateValidAddress());

        // Act
        var result = guest.ToString();

        // Assert
        Assert.Contains("Guest:", result);
        Assert.Contains("John Doe", result);
    }
}
