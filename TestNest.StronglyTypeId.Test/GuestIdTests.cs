using System;
using Xunit;
using TestNest.StronglyTypeId.StronglyTypeIds;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.Tests
{
    public class GuestIdTests
    {
        private readonly Guid _testGuid = Guid.NewGuid();
        private readonly string _testGuidString = Guid.NewGuid().ToString();

        #region Constructor Tests
        [Fact]
        public void Constructor_WithValidGuid_CreatesInstance()
        {
            var id = new GuestId(_testGuid);
            Assert.Equal(_testGuid, id.Value);
        }

        [Fact]
        public void Constructor_WithEmptyGuid_ThrowsInvalidGuidCreationException()
        {
            var ex = Assert.Throws<StronglyTypedIdException>(() => new GuestId(Guid.Empty));
            Assert.Equal(StronglyTypedIdException.ErrorCode.InvalidGuidCreation, ex.Code);
            Assert.Contains(typeof(GuestId).Name, ex.Message);
        }
        #endregion

        #region Factory Methods
        [Fact]
        public void Create_WithValidGuid_ReturnsInstance()
        {
            var id = GuestId.Create(_testGuid);
            Assert.Equal(_testGuid, id.Value);
        }

        [Fact]
        public void New_CreatesUniqueInstances()
        {
            var id1 = GuestId.New();
            var id2 = GuestId.New();
            Assert.NotEqual(id1.Value, id2.Value); // Compare GUID values directly
        }

        [Fact]
        public void Empty_ReturnsSingletonWithEmptyGuid()
        {
            var id1 = GuestId.Empty();
            var id2 = GuestId.Empty();
            Assert.Same(id1, id2); // ✅ Same instance
            Assert.Equal(Guid.Empty, id1.Value); // ✅ Empty GUID
        }
        #endregion

        #region Parsing
        [Fact]
        public void Parse_ValidGuidString_ReturnsInstance()
        {
            var id = GuestId.Parse(_testGuidString);
            Assert.Equal(_testGuidString, id.Value.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-a-guid")]
        [InlineData("00000000-0000-0000-0000-000000000000")] // Empty GUID
        public void Parse_InvalidInput_ThrowsInvalidFormatException(string invalidInput)
        {
            var ex = Assert.Throws<StronglyTypedIdException>(() => GuestId.Parse(invalidInput));
            Assert.Equal(StronglyTypedIdException.ErrorCode.InvalidFormat, ex.Code);
            Assert.Contains(invalidInput, ex.Message);
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
        public void TryParse_InvalidInput_ReturnsFalse(string? invalidInput)
        {
            var success = GuestId.TryParse(invalidInput, out var result);
            Assert.False(success);
            Assert.Null(result);
        }
        #endregion

        #region Conversion Operators
        [Fact]
        public void ExplicitOperator_FromString_WorksForValidGuid()
        {
            var id = (GuestId)_testGuidString;
            Assert.Equal(_testGuidString, id.Value.ToString());
        }

        [Fact]
        public void ExplicitOperator_FromString_ThrowsForNull()
        {
            string? nullString = null;
            Assert.Throws<ArgumentNullException>(() => (GuestId)nullString!);
        }

        [Fact]
        public void ImplicitOperator_ToGuid_ReturnsUnderlyingValue()
        {
            var id = new GuestId(_testGuid);
            Guid guid = id;
            Assert.Equal(_testGuid, guid);
        }
        #endregion

        #region Equality/Comparison
        [Fact]
        public void Equals_WithSameValue_ReturnsTrue()
        {
            var id1 = new GuestId(_testGuid);
            var id2 = new GuestId(_testGuid);
            Assert.Equal(id1, id2);
        }

        [Fact]
        public void Equals_WithDifferentValues_ReturnsFalse()
        {
            // Create two different GUIDs first
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            while (guid1 == guid2) // Ensure they're different
            {
                guid2 = Guid.NewGuid();
            }

            var id1 = new GuestId(guid1);
            var id2 = new GuestId(guid2);

            Assert.NotEqual(id1, id2);
        }

        [Fact]
        public void CompareTo_WithSameValue_ReturnsZero()
        {
            var id1 = new GuestId(_testGuid);
            var id2 = new GuestId(_testGuid);
            Assert.Equal(0, id1.CompareTo(id2));
        }
        #endregion

        #region Core Behavior
        [Fact]
        public void ToString_ReturnsGuidString()
        {
            var id = new GuestId(_testGuid);
            Assert.Equal(_testGuid.ToString(), id.ToString());
        }

        [Fact]
        public void GetHashCode_SameValues_EqualHashes()
        {
            var id1 = new GuestId(_testGuid);
            var id2 = new GuestId(_testGuid);
            Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
        }
        #endregion
    }
}