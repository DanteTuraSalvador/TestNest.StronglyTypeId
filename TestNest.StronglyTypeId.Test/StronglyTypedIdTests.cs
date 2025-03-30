using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Xunit;
using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.Test;

public class StronglyTypedIdTests
{
    #region Test Implementation
    [ModelBinder(typeof(StronglyTypedIdModelBinder<GuestId>))]
    [JsonConverter(typeof(StronglyTypedIdJsonConverter<GuestId>))]
    public sealed record GuestId : StronglyTypedId<GuestId>
    {
        public GuestId(Guid value) : base(value)
        {
            if (value == Guid.Empty)
                throw StronglyTypedIdException.InvalidGuidCreation(typeof(GuestId));
        }

        public GuestId() : this(Guid.NewGuid()) { }

        public static GuestId Create(Guid value) => new(value);

        public static GuestId Parse(string input)
        {
            if (!Guid.TryParse(input, out var guid) || guid == Guid.Empty)
                throw StronglyTypedIdException.InvalidFormat(input);

            return new(guid);
        }

        public static explicit operator GuestId(string? input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            return Parse(input);
        }

        public static new bool TryParse(
            [NotNullWhen(true)] string? input,
            [NotNullWhen(true)] out GuestId? result)
        {
            result = null;
            if (string.IsNullOrEmpty(input) || !Guid.TryParse(input, out var guid) || guid == Guid.Empty)
                return false;

            result = new GuestId(guid);
            return true;
        }

        public static GuestId New() => new(Guid.NewGuid());
        private GuestId(Guid value, bool allowEmpty) : base(value) { }

        public static GuestId Empty { get; } = new(Guid.Empty, true);
        public override string ToString() => Value.ToString();
    }
    #endregion

    #region Shared Test Data
    private readonly Guid _testGuid = Guid.NewGuid();
    private readonly string _testGuidString = Guid.NewGuid().ToString();
    #endregion

    #region Construction Tests
    [Fact]
    public void Constructor_WithValidGuid_CreatesInstance()
    {
        var id = new GuestId(_testGuid);
        Assert.Equal(_testGuid, id.Value);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsInvalidGuidException()
    {
        var ex = Assert.Throws<StronglyTypedIdException>(() => new GuestId(Guid.Empty));
        Assert.Equal(StronglyTypedIdException.ErrorCode.InvalidGuidCreation, ex.Code);
    }

    [Fact]
    public void ParameterlessConstructor_GeneratesNonEmptyGuid()
    {
        var id = new GuestId();
        Assert.NotEqual(Guid.Empty, id.Value);
    }
    #endregion

    #region Factory Method Tests
    [Fact]
    public void New_CreatesUniqueInstances()
    {
        var id1 = GuestId.New();
        var id2 = GuestId.New();
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void Empty_ShouldAlwaysReturnSameInstance()
    {
        var id1 = GuestId.Empty;
        var id2 = GuestId.Empty;

        Assert.Same(id1, id2);
        Assert.Equal(Guid.Empty, id1.Value);
    }

    [Fact]
    public void Create_ValidGuid_ReturnsInstance()
    {
        var id = GuestId.Create(_testGuid);
        Assert.Equal(_testGuid, id.Value);
    }
    #endregion

    #region Parsing Tests
    [Fact]
    public void Parse_ValidGuidString_ReturnsInstance()
    {
        var id = GuestId.Parse(_testGuidString);
        Assert.Equal(_testGuidString, id.Value.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void Parse_InvalidInput_Throws(string input)
    {
        Assert.Throws<StronglyTypedIdException>(() => GuestId.Parse(input));
    }

    [Fact]
    public void TryParse_ValidGuid_ReturnsTrueAndInstance()
    {
        var success = GuestId.TryParse(_testGuidString, out var result);
        Assert.True(success);
        Assert.Equal(_testGuidString, result?.Value.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-guid")]
    public void TryParse_InvalidInput_ReturnsFalse(string? input)
    {
        Assert.False(GuestId.TryParse(input, out _));
    }
    #endregion

    #region Conversion & Operator Tests
    [Fact]
    public void ExplicitConversion_ValidGuidString_Works()
    {
        var id = (GuestId)_testGuidString;
        Assert.Equal(_testGuidString, id.Value.ToString());
    }

    [Fact]
    public void ExplicitConversion_InvalidGuidString_Throws()
    {
        Assert.Throws<StronglyTypedIdException>(() => (GuestId)"invalid-guid");
    }

    [Fact]
    public void ExplicitConversion_NullString_Throws()
    {
        string? input = null;
        Assert.Throws<ArgumentNullException>(() => _ = (GuestId)input!);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_Works()
    {
        GuestId id = new(_testGuid);
        Guid converted = id;
        Assert.Equal(_testGuid, converted);
    }

    [Fact]
    public void EqualityOperator_EqualValues_ReturnsTrue()
    {
        var id1 = new GuestId(_testGuid);
        var id2 = new GuestId(_testGuid);
        Assert.True(id1 == id2);
    }

    [Fact]
    public void InequalityOperator_DifferentValues_ReturnsTrue()
    {
        var id1 = GuestId.New();
        var id2 = GuestId.New();
        Assert.True(id1 != id2);
    }
    #endregion

    #region Comparison Tests
    [Fact]
    public void CompareTo_SameValues_ReturnsZero()
    {
        var id1 = new GuestId(_testGuid);
        var id2 = new GuestId(_testGuid);
        Assert.Equal(0, id1.CompareTo(id2));
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var id = GuestId.New();
        Assert.True(id.CompareTo(null) > 0);
    }
    #endregion

    #region Serialization Tests
    [Fact]
    public void JsonSerialize_RoundtripsCorrectly()
    {
        var original = new GuestId(_testGuid);
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GuestId>(json);
        Assert.Equal(original, deserialized);
    }

    [Fact]
    public void JsonSerialize_Null_ReturnsNull()
    {
        GuestId? id = null;
        var json = JsonSerializer.Serialize(id);
        Assert.Equal("null", json);
    }
    #endregion

    #region Edge Cases
    [Fact]
    public void ToString_ReturnsValidGuidString()
    {
        var id = new GuestId(_testGuid);
        var result = id.ToString();

        Assert.Equal(_testGuid.ToString(), result);
        Assert.Matches(@"^[{(]?[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}[)}]?$", result);
    }

    [Fact]
    public void GetHashCode_EqualValues_EqualHashes()
    {
        var id1 = new GuestId(_testGuid);
        var id2 = new GuestId(_testGuid);
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }
    #endregion
}


//public class StronglyTypedIdTests
//{
//    [ModelBinder(typeof(StronglyTypedIdModelBinder<GuestId>))]
//    [JsonConverter(typeof(StronglyTypedIdJsonConverter<GuestId>))]
//    public sealed record GuestId : StronglyTypedId<GuestId>
//    {
//        public GuestId(Guid value) : base(value)
//        {
//            if (value == Guid.Empty)
//                throw StronglyTypedIdException.InvalidGuidCreation(typeof(GuestId));
//        }

//        public GuestId() : this(Guid.NewGuid()) { }

//        public static GuestId Create(Guid value) => new(value);

//        public static GuestId Parse(string input)
//        {
//            if (!Guid.TryParse(input, out var guid))
//                throw StronglyTypedIdException.InvalidFormat(input);

//            return new(guid); // Constructor validates empty GUID
//        }

//        public static new bool TryParse(
//            [NotNullWhen(true)] string? input,
//            [NotNullWhen(true)] out GuestId? result)
//        {
//            result = null;

//            // Fast path for null/empty
//            if (string.IsNullOrEmpty(input))
//                return false;

//            // Guid parsing
//            if (!Guid.TryParse(input, out var guid) || guid == Guid.Empty)
//                return false;

//            // Skip try/catch since constructor is the only validation
//            result = new GuestId(guid);
//            return true;
//        }

//        public static GuestId New() => new(Guid.NewGuid());

//        public static GuestId Empty { get; } = new(Guid.Empty);
//    }

//    // Test for constructor with a valid GUID
//    [Fact]
//    public void Constructor_WithNonEmptyGuid_CreatesInstance()
//    {
//        var id = new GuestId(Guid.NewGuid());
//        Assert.NotEqual(Guid.Empty, id.Value);
//    }

//    // Test for constructor with an empty GUID
//    [Fact]
//    public void Constructor_WithEmptyGuid_ThrowsInvalidGuidException()
//    {
//        var ex = Assert.Throws<StronglyTypedIdException>(() =>
//            new GuestId(Guid.Empty));

//        Assert.Equal(StronglyTypedIdException.ErrorCode.InvalidGuidCreation, ex.Code);
//        Assert.Contains("Invalid GUID provided for GuestId", ex.Message);
//    }

//    // Test for New() method - creates a new instance with a valid GUID
//    [Fact]
//    public void New_CreatesNewInstanceWithUniqueGuid()
//    {
//        var id1 = GuestId.New();
//        var id2 = GuestId.New();

//        Assert.NotEqual(id1.Value, id2.Value);
//    }

//    // Test for Empty() method - creates an instance with an empty GUID
//    [Fact]
//    public void Empty_CreatesEmptyInstance()
//    {
//        var id = GuestId.Empty();
//        Assert.Equal(Guid.Empty, id.Value);
//    }

//    // Test for TryParse with valid GUID string
//    [Fact]
//    public void TryParse_ValidGuid_ReturnsInstance()
//    {
//        var guidString = Guid.NewGuid().ToString();
//        var result = GuestId.TryParse(guidString, out var id);

//        Assert.True(result);
//        Assert.NotNull(id);
//        Assert.Equal(guidString, id?.Value.ToString());
//    }

//    // Test for TryParse with invalid GUID string
//    [Fact]
//    public void TryParse_InvalidGuid_ReturnsFalse()
//    {
//        var result = GuestId.TryParse("InvalidGuid", out var id);

//        Assert.False(result);
//        Assert.Null(id);
//    }

//    // Test for TryParse with null string
//    [Fact]
//    public void TryParse_NullGuid_ReturnsFalse()
//    {
//        var result = GuestId.TryParse(null, out var id);

//        Assert.False(result);
//        Assert.Null(id);
//    }

//    // Test for TryParse with empty string
//    [Fact]
//    public void TryParse_EmptyGuid_ReturnsFalse()
//    {
//        var result = GuestId.TryParse(string.Empty, out var id);

//        Assert.False(result);
//        Assert.Null(id);
//    }

//    // Test for TryParse with valid GUID input (Guid type)
//    [Fact]
//    public void TryParse_ValidGuidInput_ReturnsInstance()
//    {
//        var guid = Guid.NewGuid();
//        var result = GuestId.TryParse(guid, out var id);

//        Assert.True(result);
//        Assert.NotNull(id);
//        Assert.Equal(guid, id?.Value);
//    }

//    // Test for TryParse with empty GUID input (Guid type)
//    [Fact]
//    public void TryParse_EmptyGuidInput_ReturnsFalse()
//    {
//        var result = GuestId.TryParse(Guid.Empty, out var id);

//        Assert.False(result);
//        Assert.Null(id);
//    }

//    // Test for comparison between two equal instances
//    [Fact]
//    public void CompareTo_EqualIds_ReturnsZero()
//    {
//        var guid = Guid.NewGuid();
//        var id1 = new GuestId(guid);
//        var id2 = new GuestId(guid);

//        Assert.Equal(0, id1.CompareTo(id2));
//    }

//    // Test for comparison between two different instances
//    [Fact]
//    public void CompareTo_DifferentIds_ReturnsNonZero()
//    {
//        var id1 = new GuestId(Guid.NewGuid());
//        var id2 = new GuestId(Guid.NewGuid());

//        Assert.NotEqual(0, id1.CompareTo(id2));
//    }

//    // Test for equality between two equal instances
//    [Fact]
//    public void Equals_EqualIds_ReturnsTrue()
//    {
//        var guid = Guid.NewGuid();
//        var id1 = new GuestId(guid);
//        var id2 = new GuestId(guid);

//        Assert.True(id1.Equals(id2));
//    }

//    // Test for equality between two different instances
//    [Fact]
//    public void Equals_DifferentIds_ReturnsFalse()
//    {
//        var id1 = new GuestId(Guid.NewGuid());
//        var id2 = new GuestId(Guid.NewGuid());

//        Assert.False(id1.Equals(id2));
//    }

//    // Test for JsonConverter serialization and deserialization
//    [Fact]
//    public void JsonConverter_SerializesCorrectly()
//    {
//        var id = new GuestId(Guid.NewGuid());

//        var json = JsonSerializer.Serialize(id);
//        var deserializedId = JsonSerializer.Deserialize<GuestId>(json);

//        Assert.Equal(id, deserializedId);
//    }

//    // Test for deserialization failure due to invalid GUID string
//    [Fact]
//    public void JsonConverter_Deserialization_FailsOnInvalidGuid()
//    {
//        var invalidJson = "\"InvalidGuid\"";

//        var ex = Assert.Throws<StronglyTypedIdException>(() =>
//            JsonSerializer.Deserialize<GuestId>(invalidJson));

//        Assert.Equal(StronglyTypedIdException.ErrorCode.JsonDeserializationFailed, ex.Code);
//        Assert.Contains("Failed to deserialize GuestId", ex.Message);
//    }

//    // Test for model binding with a valid GUID
//    [Fact]
//    public async Task ModelBinder_BindsValidGuid_Successfully()
//    {
//        var guidValue = Guid.NewGuid().ToString();
//        var bindingContext = GetModelBindingContext(guidValue);

//        var binder = new StronglyTypedIdModelBinder<GuestId>();
//        await binder.BindModelAsync(bindingContext);

//        // Ensure binding was successful
//        Assert.True(bindingContext.Result.IsModelSet);

//        // Ensure the bound value is of the correct type
//        Assert.IsType<GuestId>(bindingContext.Result.Model);

//        // Check that the bound value matches the GUID
//        var boundValue = (GuestId)bindingContext.Result.Model!;
//        Assert.Equal(guidValue, boundValue.Value.ToString());
//    }

//    // Test for model binding failure due to empty value
//    [Fact]
//    public async Task ModelBinder_BindsEmptyValue_ThrowsException()
//    {
//        var bindingContext = GetModelBindingContext(string.Empty);

//        var binder = new StronglyTypedIdModelBinder<GuestId>();

//        var ex = await Assert.ThrowsAsync<StronglyTypedIdException>(() => binder.BindModelAsync(bindingContext));
//        Assert.Equal(StronglyTypedIdException.ErrorCode.InvalidModelValue, ex.Code);
//        Assert.Contains("Invalid value 'null' for GuestId", ex.Message);
//    }

//    // Test for model binding failure due to invalid GUID string
//    [Fact]
//    public async Task ModelBinder_BindsInvalidGuid_ThrowsException()
//    {
//        var bindingContext = GetModelBindingContext("InvalidGuid");

//        var binder = new StronglyTypedIdModelBinder<GuestId>();

//        var ex = await Assert.ThrowsAsync<StronglyTypedIdException>(() => binder.BindModelAsync(bindingContext));
//        Assert.Equal(StronglyTypedIdException.ErrorCode.InvalidModelValue, ex.Code);
//        Assert.Contains("Invalid value 'InvalidGuid' for GuestId", ex.Message);
//    }

//    // Helper method to create a ModelBindingContext
//    private static ModelBindingContext GetModelBindingContext(string value)
//    {
//        var valueProvider = new FormValueProvider(
//            BindingSource.Query,
//            new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
//            {
//        { "GuestId", value }
//            }),
//            CultureInfo.InvariantCulture);

//        var modelState = new ModelStateDictionary();
//        var bindingContext = new DefaultModelBindingContext
//        {
//            ModelName = "GuestId",
//            ModelState = modelState,
//            ValueProvider = valueProvider,
//            ActionContext = new ActionContext()
//        };
//        return bindingContext;
//    }
//}
