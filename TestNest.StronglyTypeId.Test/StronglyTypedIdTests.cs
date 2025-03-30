using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using TestNest.StronglyTypeId.Common;
using TestNest.StronglyTypeId.Exceptions;

namespace TestNest.StronglyTypeId.Test;

public class StronglyTypedIdTests
{
    #region Test Implementation
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