using TestNest.StronglyTypeId.Entities;
using TestNest.StronglyTypeId.StronglyTypeIds;
using TestNest.StronglyTypeId.ValueObjects;

namespace TestNest.StronglyTypeId.Test;

public class OrderTests
{
    private static Address CreateValidAddress() => Address.Create("123 Main St", "New York", "NY", "10001", "USA");
    private static Address CreateBillingAddress() => Address.Create("456 Billing Ave", "Chicago", "IL", "60601", "USA");

    [Fact]
    public void Create_WithValidData_ShouldCreateOrder()
    {
        // Arrange
        var customerId = CustomerId.New();
        var shippingAddress = CreateValidAddress();

        // Act
        var order = Order.Create(customerId, shippingAddress);

        // Assert
        Assert.NotNull(order);
        Assert.NotEqual(OrderId.Empty(), order.Id);
        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(shippingAddress, order.ShippingAddress);
        Assert.Null(order.BillingAddress);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(0, order.TotalAmount);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void Create_WithBillingAddress_ShouldSetBillingAddress()
    {
        // Arrange
        var customerId = CustomerId.New();
        var shippingAddress = CreateValidAddress();
        var billingAddress = CreateBillingAddress();

        // Act
        var order = Order.Create(customerId, shippingAddress, billingAddress);

        // Assert
        Assert.Equal(billingAddress, order.BillingAddress);
        Assert.True(order.HasBillingAddress);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrowArgumentException()
    {
        // Arrange
        var customerId = CustomerId.Empty();
        var shippingAddress = CreateValidAddress();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Order.Create(customerId, shippingAddress));
    }

    [Fact]
    public void Create_WithEmptyShippingAddress_ShouldThrowArgumentException()
    {
        // Arrange
        var customerId = CustomerId.New();
        var shippingAddress = Address.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Order.Create(customerId, shippingAddress));
    }

    [Fact]
    public void AddItem_WithValidData_ShouldAddItemAndCalculateTotal()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        var productId = ProductId.New();

        // Act
        order.AddItem(productId, "Test Product", 2, 25.00m);

        // Assert
        Assert.Single(order.Items);
        Assert.Equal(50.00m, order.TotalAmount);
        Assert.Equal("Test Product", order.Items[0].ProductName);
        Assert.Equal(2, order.Items[0].Quantity);
        Assert.Equal(25.00m, order.Items[0].UnitPrice);
    }

    [Fact]
    public void AddItem_WithSameProduct_ShouldIncreaseQuantity()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        var productId = ProductId.New();

        // Act
        order.AddItem(productId, "Test Product", 2, 25.00m);
        order.AddItem(productId, "Test Product", 3, 25.00m);

        // Assert
        Assert.Single(order.Items);
        Assert.Equal(5, order.Items[0].Quantity);
        Assert.Equal(125.00m, order.TotalAmount);
    }

    [Fact]
    public void AddItem_WithMultipleProducts_ShouldCalculateTotalCorrectly()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());

        // Act
        order.AddItem(ProductId.New(), "Product A", 2, 10.00m);
        order.AddItem(ProductId.New(), "Product B", 1, 30.00m);
        order.AddItem(ProductId.New(), "Product C", 3, 5.00m);

        // Assert
        Assert.Equal(3, order.Items.Count);
        Assert.Equal(65.00m, order.TotalAmount); // (2*10) + (1*30) + (3*5) = 20 + 30 + 15 = 65
    }

    [Fact]
    public void AddItem_WithEmptyProductId_ShouldThrowArgumentException()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => order.AddItem(ProductId.Empty(), "Product", 1, 10.00m));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddItem_WithInvalidProductName_ShouldThrowArgumentException(string? productName)
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => order.AddItem(ProductId.New(), productName!, 1, 10.00m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_WithInvalidQuantity_ShouldThrowArgumentException(int quantity)
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => order.AddItem(ProductId.New(), "Product", quantity, 10.00m));
    }

    [Fact]
    public void AddItem_WithNegativePrice_ShouldThrowArgumentException()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());

        // Act & Assert
        Assert.Throws<ArgumentException>(() => order.AddItem(ProductId.New(), "Product", 1, -10.00m));
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItemAndRecalculateTotal()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        var productId = ProductId.New();
        order.AddItem(productId, "Product A", 2, 10.00m);
        order.AddItem(ProductId.New(), "Product B", 1, 30.00m);

        // Act
        order.RemoveItem(productId);

        // Assert
        Assert.Single(order.Items);
        Assert.Equal(30.00m, order.TotalAmount);
    }

    [Fact]
    public void Confirm_WithItems_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        order.AddItem(ProductId.New(), "Product", 1, 10.00m);

        // Act
        order.Confirm();

        // Assert
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_WithNoItems_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Confirm());
    }

    [Fact]
    public void Ship_AfterConfirmed_ShouldChangeStatusToShipped()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        order.AddItem(ProductId.New(), "Product", 1, 10.00m);
        order.Confirm();

        // Act
        order.Ship();

        // Assert
        Assert.Equal(OrderStatus.Shipped, order.Status);
    }

    [Fact]
    public void Ship_WhenNotConfirmed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        order.AddItem(ProductId.New(), "Product", 1, 10.00m);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Ship());
    }

    [Fact]
    public void Deliver_AfterShipped_ShouldChangeStatusToDelivered()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        order.AddItem(ProductId.New(), "Product", 1, 10.00m);
        order.Confirm();
        order.Ship();

        // Act
        order.Deliver();

        // Assert
        Assert.Equal(OrderStatus.Delivered, order.Status);
    }

    [Fact]
    public void Deliver_WhenNotShipped_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        order.AddItem(ProductId.New(), "Product", 1, 10.00m);
        order.Confirm();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Deliver());
    }

    [Fact]
    public void Cancel_WhenPending_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        order.AddItem(ProductId.New(), "Product", 1, 10.00m);

        // Act
        order.Cancel();

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_WhenConfirmed_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        order.AddItem(ProductId.New(), "Product", 1, 10.00m);
        order.Confirm();

        // Act
        order.Cancel();

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_WhenShipped_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        order.AddItem(ProductId.New(), "Product", 1, 10.00m);
        order.Confirm();
        order.Ship();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.Cancel());
    }

    [Fact]
    public void AddItem_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        order.AddItem(ProductId.New(), "Product", 1, 10.00m);
        order.Confirm();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.AddItem(ProductId.New(), "New Product", 1, 5.00m));
    }

    [Fact]
    public void RemoveItem_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        var productId = ProductId.New();
        order.AddItem(productId, "Product", 1, 10.00m);
        order.Confirm();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.RemoveItem(productId));
    }

    [Fact]
    public void UpdateShippingAddress_WhenPending_ShouldUpdateAddress()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        var newAddress = Address.Create("789 New St", "Boston", "MA", "02101", "USA");

        // Act
        order.UpdateShippingAddress(newAddress);

        // Assert
        Assert.Equal(newAddress, order.ShippingAddress);
    }

    [Fact]
    public void UpdateShippingAddress_WhenShipped_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        order.AddItem(ProductId.New(), "Product", 1, 10.00m);
        order.Confirm();
        order.Ship();
        var newAddress = Address.Create("789 New St", "Boston", "MA", "02101", "USA");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => order.UpdateShippingAddress(newAddress));
    }

    [Fact]
    public void EffectiveBillingAddress_WithNoBillingAddress_ShouldReturnShippingAddress()
    {
        // Arrange
        var shippingAddress = CreateValidAddress();
        var order = Order.Create(CustomerId.New(), shippingAddress);

        // Act & Assert
        Assert.Equal(shippingAddress, order.EffectiveBillingAddress);
    }

    [Fact]
    public void EffectiveBillingAddress_WithBillingAddress_ShouldReturnBillingAddress()
    {
        // Arrange
        var shippingAddress = CreateValidAddress();
        var billingAddress = CreateBillingAddress();
        var order = Order.Create(CustomerId.New(), shippingAddress, billingAddress);

        // Act & Assert
        Assert.Equal(billingAddress, order.EffectiveBillingAddress);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var order = Order.Create(CustomerId.New(), CreateValidAddress());
        order.AddItem(ProductId.New(), "Product", 2, 25.00m);

        // Act
        var result = order.ToString();

        // Assert
        Assert.Contains("Order:", result);
        Assert.Contains("Pending", result);
    }
}
