# StronglyTypedId Library

A comprehensive .NET 8.0 library implementing the **Strongly Typed ID** pattern from Domain-Driven Design (DDD). Provides type-safe identifiers that prevent primitive obsession and enforce domain constraints.

## Features

- **Type-safe IDs** - Prevents mixing different ID types (GuestId, CustomerId, OrderId, ProductId)
- **Validation** - Built-in validation with descriptive error messages
- **Zero dependencies** - Self-contained library with no external dependencies
- **Thread-safe** - Singleton `Empty` pattern with lazy initialization
- **Domain Entities** - Example entities combining Strongly Typed IDs with Value Objects
- **118 unit tests** - Comprehensive test coverage

---

## Table of Contents

- [Installation](#installation)
- [Strongly Typed IDs](#strongly-typed-ids)
  - [Available IDs](#available-ids)
  - [Usage Examples](#usage-examples)
  - [Creating Custom IDs](#creating-custom-ids)
- [Domain Entities](#domain-entities)
  - [Customer](#customer)
  - [Guest](#guest)
  - [Order](#order)
- [Exception Handling](#exception-handling)
- [Project Structure](#project-structure)
- [Contributing](#contributing)
- [License](#license)

---

## Installation

Clone the repository and add a reference to the `TestNest.StronglyTypeId` project.

```bash
git clone https://github.com/DanteTuraSalvador/TestNest.StronglyTypeId.git
```

---

## Strongly Typed IDs

### Why Use Strongly Typed IDs?

Using `Guid` directly in entities can lead to accidental mix-ups between different ID types:

```csharp
// Dangerous: Parameters are interchangeable!
public void AssignBookingToGuest(Guid bookingId, Guid guestId)
{
    // Oops! Easy to pass IDs in wrong order
}

// Safe: Compiler prevents mixing ID types
public void AssignBookingToGuest(BookingId bookingId, GuestId guestId)
{
    // Compiler error if you pass wrong type
}
```

### Available IDs

| Type | Description |
|------|-------------|
| `GuestId` | Identifier for guest entities |
| `CustomerId` | Identifier for customer entities |
| `OrderId` | Identifier for order entities |
| `ProductId` | Identifier for product entities |
| `VisitId` | Identifier for visit entities |

### Usage Examples

#### Creating a new ID

```csharp
var guestId = GuestId.New();
var customerId = CustomerId.New();
var orderId = OrderId.New();
var productId = ProductId.New();
```

#### Parsing from a string

```csharp
// Parse (throws on invalid input)
var guestId = GuestId.Parse("b123fbb0-92a6-4f41-85b5-61a4d7306ef7");

// TryParse (safe, returns bool)
if (GuestId.TryParse("b123fbb0-92a6-4f41-85b5-61a4d7306ef7", out var result))
{
    Console.WriteLine($"Parsed: {result}");
}
```

#### Creating from existing Guid

```csharp
var guid = Guid.NewGuid();
var customerId = CustomerId.Create(guid);
```

#### Getting an empty ID (singleton)

```csharp
var emptyId = GuestId.Empty();
Console.WriteLine(emptyId);  // 00000000-0000-0000-0000-000000000000
```

#### Equality comparison

```csharp
var id1 = GuestId.New();
var id2 = GuestId.Parse(id1.ToString());

Console.WriteLine(id1 == id2);  // True
Console.WriteLine(id1.Equals(id2));  // True
```

#### Implicit conversion to Guid

```csharp
GuestId guestId = GuestId.New();
Guid guid = guestId;  // Implicit conversion
```

### Core Methods

| Method | Description |
|--------|-------------|
| `T.New()` | Creates new ID with random GUID |
| `T.Empty()` | Returns singleton empty instance |
| `T.Create(Guid)` | Creates ID from existing GUID |
| `T.Parse(string)` | Parses from GUID string (throws on failure) |
| `T.TryParse(string, out T?)` | Safe parsing without exceptions |

### Creating Custom IDs

```csharp
public sealed record BookingId : StronglyTypedId<BookingId>
{
    private static readonly Lazy<BookingId> _lazyEmpty =
        new(() => new BookingId(Guid.Empty, true), LazyThreadSafetyMode.ExecutionAndPublication);

    public static new BookingId Empty() => _lazyEmpty.Value;

    public BookingId(Guid value) : base(value)
    {
        if (value == Guid.Empty)
            throw StronglyTypedIdException.InvalidGuidCreation(typeof(BookingId));
    }

    public BookingId() : base() { }

    private BookingId(Guid value, bool _) : base(value, true) { }

    public static BookingId Create(Guid value) => new(value);
    public static new BookingId New() => new(Guid.NewGuid());

    public static BookingId Parse(string input)
    {
        if (!Guid.TryParse(input, out var guid) || guid == Guid.Empty)
            throw StronglyTypedIdException.InvalidFormat(input);
        return new(guid);
    }

    public static new bool TryParse(
        [NotNullWhen(true)] string? input,
        [NotNullWhen(true)] out BookingId? result)
    {
        result = null;
        if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out var guid) || guid == Guid.Empty)
            return false;
        result = new BookingId(guid);
        return true;
    }
}
```

---

## Domain Entities

This library includes domain entities that demonstrate how to combine **Strongly Typed IDs** with **Value Objects** (Email, PhoneNumber, Address). For detailed documentation on Value Objects, see the [TestNest.ValueObjects](https://github.com/DanteTuraSalvador/TestNest.ValueObjects) project.

### Customer

```csharp
// Create a customer with strongly typed ID and value objects
var customer = Customer.Create(
    name: "John Doe",
    email: Email.Create("john@example.com"),
    phoneNumber: PhoneNumber.Create("+1", "5551234567"),
    address: Address.Create("123 Main St", "New York", "NY", "10001", "USA")
);

// Access properties
Console.WriteLine(customer.Id);       // CustomerId (strongly typed)
Console.WriteLine(customer.Name);     // "John Doe"
Console.WriteLine(customer.Email);    // Email value object
Console.WriteLine(customer.CreatedAt);

// Update properties (fluent API)
customer
    .UpdateName("Jane Doe")
    .UpdateEmail(Email.Create("jane@example.com"))
    .UpdatePhoneNumber(PhoneNumber.Create("+44", "7911123456"))
    .UpdateAddress(Address.Create("456 Oak Ave", "London", "UK"));

// Profile checks
bool hasContact = customer.HasValidContactInfo();  // Email AND Phone
bool isComplete = customer.HasCompleteProfile();   // Contact AND Address
```

### Guest

```csharp
// Create a guest with strongly typed ID
var guest = Guest.Create(
    firstName: "John",
    lastName: "Smith",
    email: Email.Create("john.smith@example.com"),
    phoneNumber: PhoneNumber.Create("+1", "5559876543"),
    address: Address.Create("789 Pine Rd", "Chicago", "IL", "60601", "USA")
);

// Access properties
Console.WriteLine(guest.Id);        // GuestId (strongly typed)
Console.WriteLine(guest.FullName);  // "John Smith"
Console.WriteLine(guest.FirstName); // "John"
Console.WriteLine(guest.LastName);  // "Smith"

// Update properties
guest
    .UpdateFirstName("Jonathan")
    .UpdateLastName("Smithson");

Console.WriteLine(guest.FullName);  // "Jonathan Smithson"
```

### Order

```csharp
// Create an order with multiple strongly typed IDs
var customerId = CustomerId.New();
var shippingAddress = Address.Create("123 Ship St", "Portland", "OR", "97201", "USA");
var billingAddress = Address.Create("456 Bill Ave", "Seattle", "WA", "98101", "USA");

var order = Order.Create(customerId, shippingAddress, billingAddress);

// Add items using ProductId
order
    .AddItem(ProductId.New(), "Widget", quantity: 2, unitPrice: 29.99m)
    .AddItem(ProductId.New(), "Gadget", quantity: 1, unitPrice: 49.99m);

Console.WriteLine(order.Id);           // OrderId (strongly typed)
Console.WriteLine(order.CustomerId);   // CustomerId (strongly typed)
Console.WriteLine(order.TotalAmount);  // 109.97
Console.WriteLine(order.Items.Count);  // 2

// Order status workflow
order.Confirm();   // Pending -> Confirmed
order.Ship();      // Confirmed -> Shipped
order.Deliver();   // Shipped -> Delivered

// Or cancel (only before shipping)
order.Cancel();    // Pending/Confirmed -> Cancelled
```

#### Order Status Workflow

```
Pending -> Confirmed -> Shipped -> Delivered
    |          |
    v          v
 Cancelled  Cancelled
```

---

## Exception Handling

All exceptions follow a consistent pattern with error codes and factory methods.

### StronglyTypedIdException

```csharp
try
{
    var id = GuestId.Parse("invalid-guid");
}
catch (StronglyTypedIdException ex)
{
    Console.WriteLine(ex.Code);     // ErrorCode.InvalidFormat
    Console.WriteLine(ex.Message);  // "The value 'invalid-guid' is not a valid GUID format"
}
```

Error codes: `NullInstanceCreation`, `InvalidGuidCreation`, `InvalidFormat`, `NullId`

---

## Project Structure

```
TestNest.StronglyTypeId/
├── TestNest.StronglyTypeId/
│   ├── Common/
│   │   ├── StronglyTypedId.cs          # Base class for strongly typed IDs
│   │   └── ValueObject.cs              # Base class for value objects
│   ├── Exceptions/
│   │   ├── StronglyTypeIdException.cs  # ID validation exceptions
│   │   ├── EmailException.cs           # Email validation exceptions
│   │   ├── PhoneNumberException.cs     # Phone validation exceptions
│   │   └── AddressException.cs         # Address validation exceptions
│   ├── StronglyTypeIds/
│   │   ├── GuestId.cs                  # Guest identifier
│   │   ├── CustomerId.cs               # Customer identifier
│   │   ├── OrderId.cs                  # Order identifier
│   │   ├── ProductId.cs                # Product identifier
│   │   └── VisitId.cs                  # Visit identifier
│   ├── ValueObjects/
│   │   ├── Email.cs                    # Email value object
│   │   ├── PhoneNumber.cs              # Phone number value object
│   │   └── Address.cs                  # Address value object
│   └── Entities/
│       ├── Customer.cs                 # Customer domain entity
│       ├── Guest.cs                    # Guest domain entity
│       └── Order.cs                    # Order domain entity
│
├── TestNest.StronglyTypeId.Test/
│   ├── StronglyTypedIdTests.cs         # Base class tests
│   ├── GuestIdTests.cs                 # GuestId tests
│   ├── CustomerTests.cs                # Customer entity tests
│   ├── GuestEntityTests.cs             # Guest entity tests
│   └── OrderTests.cs                   # Order entity tests
│
├── TestNest.StronglyTypeId.Console/
│   └── Program.cs                      # Interactive demo application
│
├── README.md
└── LICENSE
```

---

## Design Principles

- **Immutability** - All IDs are immutable once created
- **Value Semantics** - IDs are compared by their values, not references
- **Self-Validation** - IDs validate themselves during construction
- **Fail Fast** - Invalid data throws immediately with descriptive messages
- **Factory Methods** - Multiple creation patterns (Create, TryCreate, Parse, TryParse)
- **Singleton Empty** - Thread-safe lazy singleton for empty instances

---

## Project Management

Track project progress, features, and user stories on our Azure DevOps board:

- [Azure DevOps Board](https://dev.azure.com/tengtium-io/StronglyTypedId) - Epics, Features, and User Stories

---

## Related Projects

- [TestNest.SmartEnums](https://github.com/DanteTuraSalvador/TestNest.SmartEnums) - SmartEnum pattern implementation with state machine (CheckInOut, Visit entity)
- [TestNest.ValueObjects](https://github.com/DanteTuraSalvador/TestNest.ValueObjects) - Value Object pattern implementation (Email, PhoneNumber, Address, Money, Currency, Price)

---

## Contributing

Pull requests are welcome! Please:

- Maintain test coverage
- Follow existing code style
- Add documentation for new features

---

## License

This project is open-source and free to use.
