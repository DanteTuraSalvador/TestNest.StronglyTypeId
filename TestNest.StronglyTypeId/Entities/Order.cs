using TestNest.StronglyTypeId.StronglyTypeIds;
using TestNest.StronglyTypeId.ValueObjects;

namespace TestNest.StronglyTypeId.Entities;

public sealed class Order
{
    public OrderId Id { get; }
    public CustomerId CustomerId { get; }
    public Address ShippingAddress { get; private set; }
    public Address? BillingAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order(
        OrderId id,
        CustomerId customerId,
        Address shippingAddress,
        Address? billingAddress)
    {
        Id = id;
        CustomerId = customerId;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        Status = OrderStatus.Pending;
        TotalAmount = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public static Order Create(
        CustomerId customerId,
        Address shippingAddress,
        Address? billingAddress = null)
    {
        if (customerId == CustomerId.Empty())
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

        if (shippingAddress.IsEmpty())
            throw new ArgumentException("Shipping address cannot be empty", nameof(shippingAddress));

        return new Order(
            OrderId.New(),
            customerId,
            shippingAddress,
            billingAddress);
    }

    public static Order Create(
        OrderId id,
        CustomerId customerId,
        Address shippingAddress,
        Address? billingAddress = null)
    {
        if (customerId == CustomerId.Empty())
            throw new ArgumentException("Customer ID cannot be empty", nameof(customerId));

        if (shippingAddress.IsEmpty())
            throw new ArgumentException("Shipping address cannot be empty", nameof(shippingAddress));

        return new Order(
            id,
            customerId,
            shippingAddress,
            billingAddress);
    }

    public Order AddItem(ProductId productId, string productName, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to a non-pending order");

        if (productId == ProductId.Empty())
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            _items.Add(new OrderItem(productId, productName, quantity, unitPrice));
        }

        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Order RemoveItem(ProductId productId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot remove items from a non-pending order");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            _items.Remove(item);
            RecalculateTotal();
            UpdatedAt = DateTime.UtcNow;
        }

        return this;
    }

    public Order UpdateShippingAddress(Address newAddress)
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot update shipping address after order has been shipped");

        if (newAddress.IsEmpty())
            throw new ArgumentException("Shipping address cannot be empty", nameof(newAddress));

        ShippingAddress = newAddress;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Order UpdateBillingAddress(Address? newAddress)
    {
        BillingAddress = newAddress;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Order Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot confirm an empty order");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Order Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be shipped");

        Status = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Order Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be delivered");

        Status = OrderStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public Order Cancel()
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel an order that has been shipped or delivered");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }

    public bool HasBillingAddress => BillingAddress is not null && !BillingAddress.IsEmpty();

    public Address EffectiveBillingAddress => HasBillingAddress ? BillingAddress! : ShippingAddress;

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.TotalPrice);
    }

    public override string ToString()
        => $"Order: {Id} - {Status} - {TotalAmount:C}";
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}

public sealed class OrderItem
{
    public ProductId ProductId { get; }
    public string ProductName { get; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; }
    public decimal TotalPrice => Quantity * UnitPrice;

    public OrderItem(ProductId productId, string productName, int quantity, decimal unitPrice)
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));

        Quantity = newQuantity;
    }
}
