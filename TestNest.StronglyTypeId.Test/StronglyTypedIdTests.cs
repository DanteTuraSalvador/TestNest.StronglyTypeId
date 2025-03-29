using FluentAssertions;

using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using TestNest.Domain.Exceptions;
using TestNest.StronglyTypeId.Common;

namespace TestNest.StronglyTypeId.Test;
public class StronglyTypedIdTests
{
    #region Test Implementation (Fully Compliant with Record Requirements)

    private sealed record TestId : StronglyTypedId<TestId>
    {
        public TestId() : base() { }
        public TestId(Guid value) : base(value) { }
        public static explicit operator TestId?(string? input) =>
            TryParse(input, out var id) ? id : null;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void ParameterlessConstructor_ShouldCreateNewGuid()
    {
        // Act
        var id1 = new TestId();
        var id2 = new TestId();

        // Assert
        id1.Value.Should().NotBeEmpty();
        id2.Value.Should().NotBeEmpty();
        id1.Value.Should().NotBe(id2.Value);
    }


    [Fact]
    public void Constructor_WithGuid_ShouldSetValue()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = new TestId(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void RecordClone_ShouldWork()
    {
        // Arrange
        var original = new TestId(Guid.NewGuid());

        // Act
        var cloned = original with { };

        // Assert
        cloned.Value.Should().Be(original.Value);
        cloned.Should().NotBeSameAs(original);
        cloned.Should().Be(original); // Value equality
    }


    #endregion

    #region Conversion Tests

    [Fact]
    public void ExplicitConversion_FromString_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var guidString = guid.ToString();

        // Act
        var id = (TestId?)guidString;

        // Assert
        id.Should().NotBeNull();
        id!.Value.Should().Be(guid);
    }


    [Fact]
    public void ExplicitConversion_FromInvalidString_ShouldReturnNull()
    {
        // Act
        var id = (TestId?)"invalid-guid";

        // Assert
        id.Should().BeNull();
    }

    #endregion

    #region JSON Conversion Tests

    [Fact]
    public void JsonSerialization_ShouldWork()
    {
        // Arrange
        var id = new TestId(Guid.NewGuid());
        var options = new JsonSerializerOptions
        {
            Converters = { new StronglyTypedIdJsonConverter<TestId>() }
        };

        // Act
        var json = JsonSerializer.Serialize(id, options);
        var deserialized = JsonSerializer.Deserialize<TestId>(json, options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Value.Should().Be(id.Value);
    }

    [Fact]
    public void JsonDeserialization_WithInvalidGuid_ShouldThrowCustomException()
    {
        // Arrange
        const string invalidJson = "\"not-a-guid\"";
        var options = new JsonSerializerOptions
        {
            Converters = { new StronglyTypedIdJsonConverter<TestId>() }
        };

        // Act
        var exception = Record.Exception(() =>
            JsonSerializer.Deserialize<TestId>(invalidJson, options));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<StronglyTypedIdException.Json.Deserialization>();
        exception.Message.Should().Contain("Couldn't deserialize TestId from JSON");
        exception.Message.Should().Contain("not-a-guid");
    }




    #endregion

    #region Model Binding Tests

    [Fact]
    public async Task ModelBinder_ShouldBindFromRoute()
    {
        // Arrange
        var binder = new StronglyTypedIdModelBinder<TestId>();
        var valueProvider = new Mock<IValueProvider>();
        var bindingContext = new DefaultModelBindingContext();

        var guid = Guid.NewGuid();
        valueProvider.Setup(v => v.GetValue("testParam"))
            .Returns(new ValueProviderResult(guid.ToString()));

        bindingContext.ModelName = "testParam";
        bindingContext.ValueProvider = valueProvider.Object;
        bindingContext.ModelState = new ModelStateDictionary();

        // Act
        await binder.BindModelAsync(bindingContext);

        // Assert
        bindingContext.Result.IsModelSet.Should().BeTrue();
        var model = bindingContext.Result.Model as TestId;
        model.Should().NotBeNull();
        model!.Value.Should().Be(guid);
    }

    [Fact]
    public async Task ModelBinder_WithInvalidGuid_ShouldThrowCustomException()
    {
        // Arrange
        var binder = new StronglyTypedIdModelBinder<TestId>();
        var valueProvider = new Mock<IValueProvider>();
        var bindingContext = new DefaultModelBindingContext();

        valueProvider.Setup(v => v.GetValue("testParam"))
            .Returns(new ValueProviderResult("invalid-guid"));

        bindingContext.ModelName = "testParam";
        bindingContext.ValueProvider = valueProvider.Object;
        bindingContext.ModelState = new ModelStateDictionary();

        // Act
        var exception = await Record.ExceptionAsync(() =>
            binder.BindModelAsync(bindingContext));

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<StronglyTypedIdException.ModelBinder.InvalidFormat>();
        exception.Message.Should().Contain("The provided value 'invalid-guid'");
        exception.Message.Should().Contain("testParam");
    }


    [Fact]
    public async Task ModelBinder_WithInvalidGuid_ShouldFail()
    {
        // Arrange
        var binder = new StronglyTypedIdModelBinder<TestId>();
        var valueProvider = new Mock<IValueProvider>();
        var bindingContext = new DefaultModelBindingContext();

        valueProvider.Setup(v => v.GetValue("testParam"))
            .Returns(new ValueProviderResult("invalid-guid"));

        bindingContext.ModelName = "testParam";
        bindingContext.ValueProvider = valueProvider.Object;
        bindingContext.ModelState = new ModelStateDictionary();

        // Act & Assert
        await Assert.ThrowsAsync<StronglyTypedIdException.ModelBinder.InvalidFormat>(
            () => binder.BindModelAsync(bindingContext));
    }

    #endregion

    #region Model Binding Tests - Complete API Call Coverage

    [Fact]
    public async Task BindModelAsync_ValidGuidInRoute_ShouldBindSuccessfully()
    {
        // Arrange
        var binder = new StronglyTypedIdModelBinder<TestId>();
        var guid = Guid.NewGuid();
        var context = CreateBindingContext("id", guid.ToString());

        // Act
        await binder.BindModelAsync(context);

        // Assert
        context.Result.IsModelSet.Should().BeTrue();
        (context.Result.Model as TestId)?.Value.Should().Be(guid);
    }

    [Fact]
    public async Task BindModelAsync_ValidGuidInQueryString_ShouldBindSuccessfully()
    {
        // Arrange
        var binder = new StronglyTypedIdModelBinder<TestId>();
        var guid = Guid.NewGuid();
        var context = CreateBindingContext("id", guid.ToString());

        // Act
        await binder.BindModelAsync(context);

        // Assert
        context.Result.IsModelSet.Should().BeTrue();
        (context.Result.Model as TestId)?.Value.Should().Be(guid);
    }

    [Fact]
    public async Task BindModelAsync_EmptyGuid_ShouldBindToEmptyInstance()
    {
        // Arrange
        var binder = new StronglyTypedIdModelBinder<TestId>();
        var context = CreateBindingContext("id", Guid.Empty.ToString());

        // Act
        await binder.BindModelAsync(context);

        // Assert
        context.Result.IsModelSet.Should().BeTrue();
        (context.Result.Model as TestId)?.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task BindModelAsync_NullBindingContext_ShouldThrowNullBindingContextException()
    {
        // Arrange
        var binder = new StronglyTypedIdModelBinder<TestId>();

        // Act & Assert
        await Assert.ThrowsAsync<StronglyTypedIdException.ModelBinder.NullBindingContext>(
            () => binder.BindModelAsync(null!));
    }

    [Fact]
    public async Task BindModelAsync_MissingValue_ShouldThrowMissingValueException()
    {
        // Arrange
        var binder = new StronglyTypedIdModelBinder<TestId>();
        var context = CreateBindingContext("id", ValueProviderResult.None);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<StronglyTypedIdException.ModelBinder.MissingValue>(
            () => binder.BindModelAsync(context));

        ex.Message.Should().Contain("id");
    }

    [Fact]
    public async Task BindModelAsync_InvalidGuidFormat_ShouldThrowInvalidFormatException()
    {
        // Arrange
        var binder = new StronglyTypedIdModelBinder<TestId>();
        var context = CreateBindingContext("id", "not-a-guid");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<StronglyTypedIdException.ModelBinder.InvalidFormat>(
            () => binder.BindModelAsync(context));

        ex.Message.Should().Contain("id");
        ex.Message.Should().Contain("not-a-guid");
    }

    [Fact]
    public async Task BindModelAsync_CaseInsensitiveGuid_ShouldBindSuccessfully()
    {
        // Arrange
        var binder = new StronglyTypedIdModelBinder<TestId>();
        var guid = Guid.NewGuid();
        var context = CreateBindingContext("id", guid.ToString().ToUpper());

        // Act
        await binder.BindModelAsync(context);

        // Assert
        context.Result.IsModelSet.Should().BeTrue();
        (context.Result.Model as TestId)?.Value.Should().Be(guid);
    }

    [Fact]
    public async Task BindModelAsync_WithBracesGuidFormat_ShouldBindSuccessfully()
    {
        // Arrange
        var binder = new StronglyTypedIdModelBinder<TestId>();
        var guid = Guid.NewGuid();
        var context = CreateBindingContext("id", $"{{{guid}}}");

        // Act
        await binder.BindModelAsync(context);

        // Assert
        context.Result.IsModelSet.Should().BeTrue();
        (context.Result.Model as TestId)?.Value.Should().Be(guid);
    }

    #endregion

    #region JSON Conversion Tests - API Request/Response Scenarios

    [Fact]
    public void JsonConverter_ShouldSerializeToGuidString()
    {
        // Arrange
        var id = new TestId(Guid.NewGuid());
        var options = new JsonSerializerOptions
        {
            Converters = { new StronglyTypedIdJsonConverter<TestId>() }
        };

        // Act
        var json = JsonSerializer.Serialize(id, options);

        // Assert
        json.Should().Be($"\"{id.Value}\"");
    }

    [Fact]
    public void JsonConverter_ShouldDeserializeFromGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var options = new JsonSerializerOptions
        {
            Converters = { new StronglyTypedIdJsonConverter<TestId>() }
        };

        // Act
        var result = JsonSerializer.Deserialize<TestId>($"\"{guid}\"", options);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(guid);
    }

    #endregion

    #region Helper Methods

    private static DefaultModelBindingContext CreateBindingContext(string modelName, string value)
    {
        var valueProvider = new Mock<IValueProvider>();
        valueProvider.Setup(v => v.GetValue(modelName))
            .Returns(new ValueProviderResult(value));

        return new DefaultModelBindingContext
        {
            ModelName = modelName,
            ValueProvider = valueProvider.Object,
            ModelState = new ModelStateDictionary()
        };
    }

    private static DefaultModelBindingContext CreateBindingContext(string modelName, ValueProviderResult result)
    {
        var valueProvider = new Mock<IValueProvider>();
        valueProvider.Setup(v => v.GetValue(modelName))
            .Returns(result);

        return new DefaultModelBindingContext
        {
            ModelName = modelName,
            ValueProvider = valueProvider.Object,
            ModelState = new ModelStateDictionary()
        };
    }

    #endregion

    // Add these tests to your existing StronglyTypedIdTests class

    #region Comparison Tests

    [Fact]
    public void CompareTo_WithSameType_ReturnsCorrectComparisonResult()
    {
        // Arrange
        var id1 = new TestId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var id2 = new TestId(Guid.Parse("00000000-0000-0000-0000-000000000002"));

        // Act & Assert
        id1.CompareTo(id2).Should().BeNegative();
        id2.CompareTo(id1).Should().BePositive();
        id1.CompareTo(id1).Should().Be(0);
    }

    [Fact]
    public void CompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var id = new TestId();

        // Act & Assert
        id.CompareTo(null).Should().BePositive();
    }

    [Fact]
    public void NonGenericCompareTo_WithSameType_ReturnsCorrectComparisonResult()
    {
        // Arrange
        var id1 = new TestId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        var id2 = new TestId(Guid.Parse("00000000-0000-0000-0000-000000000002"));
        var comparable = (IComparable)id1;

        // Act & Assert
        comparable.CompareTo(id2).Should().BeNegative();
        comparable.CompareTo(id1).Should().Be(0);
    }

    [Fact]
    public void NonGenericCompareTo_WithDifferentType_ThrowsArgumentException()
    {
        // Arrange
        var id = new TestId();
        var differentId = new OtherTestId(); // Different strongly-typed ID
        var comparable = (IComparable)id;

        // Act
        Action act = () => comparable.CompareTo(differentId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Object must be of type {typeof(TestId).Name}*");
    }

    [Fact]
    public void NonGenericCompareTo_WithNull_ReturnsPositive()
    {
        // Arrange
        var id = new TestId();
        var comparable = (IComparable)id;

        // Act
        var result = comparable.CompareTo(null);

        // Assert
        result.Should().Be(1);  // Explicitly expect 1 for null comparison
    }

    // Helper record for testing
    private sealed record OtherTestId : StronglyTypedId<OtherTestId>
    {
        public OtherTestId() : base() { }
        public OtherTestId(Guid value) : base(value) { }
    }

    #endregion
}