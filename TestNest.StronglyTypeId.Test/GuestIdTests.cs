using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;
using TestNest.StronglyTypeId.StronglyTypeIds;

namespace TestNest.StronglyTypeId.Tests
{
    public class GuestIdTests
    {
        private readonly Guid _testGuid = Guid.NewGuid();
        private readonly string _testGuidString = Guid.NewGuid().ToString();

        private readonly JsonSerializerOptions _jsonOptions;

        public GuestIdTests()
        {
            _jsonOptions = new JsonSerializerOptions();
            _jsonOptions.Converters.Add(new StronglyTypedIdJsonConverter<GuestId>());
        }


        #region Construction Tests
        [Fact]
        public void Constructor_WithValidGuid_SetsValue()
        {
            var id = new GuestId(_testGuid);
            Assert.Equal(_testGuid, id.Value);
        }

        [Fact]
        public void Constructor_WithEmptyGuid_ThrowsInvalidGuidCreation()
        {
            var ex = Assert.Throws<StronglyTypedIdException>(() =>
                new GuestId(Guid.Empty));
            Assert.Equal(StronglyTypedIdException.ErrorCode.InvalidGuidCreation, ex.Code);
        }

        [Fact]
        public void ParameterlessConstructor_CreatesNonEmptyId()
        {
            var id = new GuestId();
            Assert.NotEqual(Guid.Empty, id.Value);
        }
        #endregion

        #region Factory Method Tests
        [Fact]
        public void Create_WithValidGuid_ReturnsInstance()
        {
            var id = GuestId.Create(_testGuid);
            Assert.Equal(_testGuid, id.Value);
        }

        [Fact]
        public void New_ReturnsUniqueInstances()
        {
            var id1 = GuestId.New();
            var id2 = GuestId.New();
            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void Empty_ReturnsSingletonInstance()
        {
            var id1 = GuestId.Empty;
            var id2 = GuestId.Empty;
            Assert.Same(id1, id2);
            Assert.Equal(Guid.Empty, id1.Value);
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
        [InlineData("invalid-guid")]
        [InlineData("")]
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
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public void TryParse_InvalidInput_ReturnsFalse(string? input)
        {
            Assert.False(GuestId.TryParse(input, out var result));
            Assert.Null(result);
        }
        #endregion

        #region Conversion Tests
        [Fact]
        public void ExplicitConversion_ValidGuidString_Works()
        {
            var id = (GuestId)_testGuidString;
            Assert.Equal(_testGuidString, id.Value.ToString());
        }

        [Fact]
        public void ExplicitConversion_Null_ThrowsArgumentNullException()
        {
            string? nullString = null;
            var ex = Assert.Throws<ArgumentNullException>(() => (GuestId)nullString);
            Assert.Equal("input", ex.ParamName);
        }

        [Fact]
        public void ExplicitConversion_InvalidGuid_Throws()
        {
            Assert.Throws<StronglyTypedIdException>(() => (GuestId)"invalid-guid");
        }

        [Fact]
        public void ToString_ReturnsGuidString()
        {
            var id = new GuestId(_testGuid);
            Assert.Equal(_testGuid.ToString(), id.ToString());
        }
        #endregion

        #region Equality Tests
        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var id1 = new GuestId(_testGuid);
            var id2 = new GuestId(_testGuid);
            Assert.Equal(id1, id2);
            Assert.True(id1 == id2);
        }

        [Fact]
        public void Equals_DifferentValues_ReturnsFalse()
        {
            var id1 = GuestId.New();
            var id2 = GuestId.New();
            Assert.NotEqual(id1, id2);
            Assert.True(id1 != id2);
        }

        [Fact]
        public void GetHashCode_EqualValues_ReturnsSameHash()
        {
            var id1 = new GuestId(_testGuid);
            var id2 = new GuestId(_testGuid);
            Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
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

        [Fact]
        public void IComparable_CompareTo_WrongType_Throws()
        {
            var id = GuestId.New();
            Assert.Throws<ArgumentException>(() =>
                ((IComparable)id).CompareTo("not-a-guid"));
        }
        #endregion

        #region Serialization Tests

        [Fact]
        public void JsonSerialize_ProducesCorrectFormat()
        {
            var id = new GuestId(_testGuid);
            var json = JsonSerializer.Serialize(id, _jsonOptions);
            Assert.Equal($"\"{_testGuid}\"", json);
        }

        [Fact]
        public void Empty_AlwaysReturnsSameInstance()
        {
            var id1 = GuestId.Empty;
            var id2 = GuestId.Empty;
            Assert.Same(id1, id2);
        }

        [Fact]
        public void ExplicitConversion_EmptyGuidString_Throws()
        {
            Assert.Throws<StronglyTypedIdException>(
                () => (GuestId)"00000000-0000-0000-0000-000000000000");
        }

        [Fact]
        public void JsonSerializer_WithOptions_UseCorrectConverter()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new StronglyTypedIdJsonConverter<GuestId>());

            var id = GuestId.New();
            var json = JsonSerializer.Serialize(id, options);
            var deserialized = JsonSerializer.Deserialize<GuestId>(json, options);

            Assert.Equal(id, deserialized);
        }


        [Fact]
        public void JsonSerialize_RoundtripsCorrectly()
        {
            // Arrange
            var options = new JsonSerializerOptions
            {
                Converters = { new StronglyTypedIdJsonConverter<GuestId>() }
            };
            var original = new GuestId(_testGuid);

            // Act
            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<GuestId>(json, options);

            // Assert
            Assert.Equal(original.Value, deserialized?.Value); // Compare GUID values directly
            Assert.Equal(original, deserialized); // Compare GuestId objects
        }

        [Fact]
        public void JsonSerialize_Null_ReturnsNull()
        {
            GuestId? id = null;
            var json = JsonSerializer.Serialize(id);
            Assert.Equal("null", json);
        }

       [Fact]
        public void JsonDeserialize_InvalidGuid_Throws()
        {
            var json = "\"invalid-guid\"";
            Assert.Throws<StronglyTypedIdException>(() => 
                JsonSerializer.Deserialize<GuestId>(json));
        }

        [Fact]
        public void TryParse_Performance_ValidGuid()
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                GuestId.TryParse(_testGuidString, out _);
            }
            sw.Stop();
            Assert.True(sw.ElapsedMilliseconds < 100); // Adjust threshold
        }

        [Fact]
        public void JsonSerialize_WithAttribute_ProducesCorrectFormat()
        {
            var id = new GuestId(_testGuid);
            var json = JsonSerializer.Serialize(id); // Relies on the [JsonConverter] attribute
            Assert.Equal($"\"{_testGuid}\"", json);
        }

        [Fact]
        public void GuestId_HasJsonConverterAttribute()
        {
            var attribute = typeof(GuestId)
                .GetCustomAttributes(typeof(JsonConverterAttribute), true)
                .FirstOrDefault() as JsonConverterAttribute;

            Assert.NotNull(attribute);
            Assert.Equal(typeof(StronglyTypedIdJsonConverter<GuestId>), attribute.ConverterType);
        }
        #endregion

        #region Model Binding Tests

        [Fact]
        public async Task ModelBinder_ValidGuidString_BindsSuccessfully()
        {
            // Arrange
            var testGuid = Guid.NewGuid().ToString();
            var binder = new StronglyTypedIdModelBinder<GuestId>();
            var valueProvider = new TestValueProvider(testGuid);
            var bindingContext = CreateBindingContext(valueProvider);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.True(bindingContext.Result.IsModelSet);
            var result = Assert.IsType<GuestId>(bindingContext.Result.Model);
            Assert.Equal(testGuid, result.ToString());

            // ModelState will contain the field but should have no errors
            Assert.True(bindingContext.ModelState.ContainsKey("testField"));
            Assert.Empty(bindingContext.ModelState["testField"].Errors);
        }
        [Theory]
        [InlineData(null, "A value is required")]
        [InlineData("", "Value cannot be null or empty")]
        [InlineData("invalid-guid", "Invalid GUID format")]
        [InlineData("00000000-0000-0000-0000-000000000000", "GUID cannot be empty")]
        public async Task ModelBinder_InvalidInput_AddsModelError(string input, string expectedError)
        {
            // Arrange
            var binder = new StronglyTypedIdModelBinder<GuestId>();
            var valueProvider = new TestValueProvider(input);
            var bindingContext = CreateBindingContext(valueProvider);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(bindingContext.Result.IsModelSet);
            Assert.True(bindingContext.ModelState.ErrorCount > 0,
                $"Expected model errors for input: '{input}'");

            var error = bindingContext.ModelState["testField"].Errors[0].ErrorMessage;
            Assert.Contains(expectedError, error);
        }

        [Fact]
        public async Task ModelBinder_NullValueProviderResult_AddsRequiredError()
        {
            // Arrange
            var binder = new StronglyTypedIdModelBinder<GuestId>();
            var valueProvider = new TestValueProvider(null); // None result
            var bindingContext = CreateBindingContext(valueProvider);

            // Act
            await binder.BindModelAsync(bindingContext);

            // Assert
            Assert.False(bindingContext.Result.IsModelSet);
            Assert.NotEmpty(bindingContext.ModelState);
            Assert.Equal("A value is required for GuestId",
                bindingContext.ModelState["testField"].Errors[0].ErrorMessage);
        }

        //[Fact]
        //public async Task ModelBinder_NullValueProviderResult_FailsSilently()
        //{
        //    // Arrange
        //    var binder = new StronglyTypedIdModelBinder<GuestId>();
        //    var valueProvider = new TestValueProvider(null); // None result
        //    var bindingContext = CreateBindingContext(valueProvider);

        //    // Act
        //    await binder.BindModelAsync(bindingContext);

        //    // Assert
        //    Assert.False(bindingContext.Result.IsModelSet);
        //    Assert.Empty(bindingContext.ModelState);
        //}

        [Fact]
        public void ModelBinder_RegisteredViaAttribute()
        {
            // Arrange
            var attribute = typeof(GuestId)
                .GetCustomAttributes(typeof(ModelBinderAttribute), true)
                .FirstOrDefault() as ModelBinderAttribute;

            // Assert
            Assert.NotNull(attribute);
            Assert.Equal(typeof(StronglyTypedIdModelBinder<GuestId>), attribute.BinderType);
        }

        private static ModelBindingContext CreateBindingContext(IValueProvider valueProvider)
        {
            var bindingContext = DefaultModelBindingContext.CreateBindingContext(
                new ActionContext(),
                valueProvider,
                new EmptyModelMetadataProvider().GetMetadataForType(typeof(GuestId)),
                new BindingInfo(),
                "testField");

            return bindingContext;
        }

        private class TestValueProvider : IValueProvider
        {
            private readonly string _value;

            public TestValueProvider(string value)
            {
                _value = value;
            }

            public bool ContainsPrefix(string prefix) => true;

            public ValueProviderResult GetValue(string key)
            {
                return _value != null
                    ? new ValueProviderResult(_value)
                    : ValueProviderResult.None;
            }
        }

        #endregion

        #region Edge Cases
        [Fact]
        public void Empty_CanBeUsedInCollections()
        {
            var dict = new System.Collections.Generic.Dictionary<GuestId, string>
            {
                [GuestId.Empty] = "test"
            };
            Assert.Equal("test", dict[GuestId.Empty]);
        }

        [Fact]
        public void TryParse_WithWhitespace_ReturnsFalse()
        {
            Assert.False(GuestId.TryParse("   ", out _));
        }
        #endregion
    }
}