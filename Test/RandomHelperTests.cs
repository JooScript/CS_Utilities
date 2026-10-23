using Utils.Randomness;

namespace Utils.Test;

public class RandomHelperTests
{
    [Fact]
    public void GetPastDate_ReturnsDateWithinRequestedYears()
    {
        var before = DateTime.Now.AddDays(-366);

        var result = RandomHelper.GetPastDate(1);

        Assert.True(result < DateTime.Now);
        Assert.True(result >= before);
    }

    [Fact]
    public void GetPastDate_WhenYearsNotPositive_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomHelper.GetPastDate(0));
    }

    [Fact]
    public void GetDateBetween_ReturnsDateWithinRange()
    {
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 10);

        var result = RandomHelper.GetDateBetween(start, end);

        Assert.InRange(result, start, end);
    }

    [Fact]
    public void GetDateBetween_WhenStartNotBeforeEnd_Throws()
    {
        var date = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() => RandomHelper.GetDateBetween(date, date));
    }

    [Fact]
    public void GetItem_ReturnsItemFromList()
    {
        var items = new[] { "a", "b", "c" };

        var result = RandomHelper.GetItem(items);

        Assert.Contains(result, items);
    }

    [Fact]
    public void GetItem_WhenListNullOrEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() => RandomHelper.GetItem(Array.Empty<string>()));
        Assert.Throws<ArgumentException>(() => RandomHelper.GetItem<string>(null!));
    }

    [Fact]
    public void GetEnumValue_ReturnsDefinedValue()
    {
        var result = RandomHelper.GetEnumValue<DayOfWeek>();

        Assert.True(Enum.IsDefined(result));
    }

    [Fact]
    public void GetBool_UsesProbabilityBounds()
    {
        Assert.True(RandomHelper.GetBool(1));
        Assert.False(RandomHelper.GetBool(0));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void GetBool_WhenProbabilityInvalid_Throws(double probability)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomHelper.GetBool(probability));
    }

    [Fact]
    public void GetInt_ReturnsValueWithinRange()
    {
        var result = RandomHelper.GetInt(5, 6);

        Assert.Equal(5, result);
    }

    [Fact]
    public void GetDouble_ReturnsValueWithinRange()
    {
        var result = RandomHelper.GetDouble(1.5, 2.5);

        Assert.InRange(result, 1.5, 2.5);
    }

    [Fact]
    public void GetDouble_WhenRangeInvalid_Throws()
    {
        Assert.Throws<ArgumentException>(() => RandomHelper.GetDouble(2, 1));
    }

    [Fact]
    public void GetString_ReturnsRequestedLengthFromCharacterSet()
    {
        var result = RandomHelper.GetString(12, "ab");

        Assert.Equal(12, result.Length);
        Assert.All(result, c => Assert.Contains(c, "ab"));
    }

    [Fact]
    public void GetString_WhenInvalidArguments_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomHelper.GetString(0));
        Assert.Throws<ArgumentException>(() => RandomHelper.GetString(1, ""));
    }

    [Fact]
    public void GetAlphanumeric_ReturnsRequestedLength()
    {
        Assert.Equal(16, RandomHelper.GetAlphanumeric(16).Length);
    }

    [Fact]
    public void GetColor_ReturnsOpaqueColor()
    {
        var result = RandomHelper.GetColor();

        Assert.Equal(255, result.A);
    }

    [Fact]
    public void Shuffle_KeepsAllItems()
    {
        var items = new List<int> { 1, 2, 3, 4, 5 };

        RandomHelper.Shuffle(items);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, items.OrderBy(x => x));
    }

    [Fact]
    public void Shuffle_WhenNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RandomHelper.Shuffle<int>(null!));
    }

    [Fact]
    public void Shuffled_ReturnsCopyWithSameItems()
    {
        var items = new List<int> { 1, 2, 3 };

        var result = RandomHelper.Shuffled(items);

        Assert.NotSame(items, result);
        Assert.Equal(items.OrderBy(x => x), result.OrderBy(x => x));
    }

    [Fact]
    public void Shuffled_WhenNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RandomHelper.Shuffled<int>(null!));
    }

    [Fact]
    public void GetWeighted_ReturnsOnlyItemWithWeight()
    {
        var items = new[] { "first", "second" };

        var result = RandomHelper.GetWeighted(items, new[] { 0d, 1d });

        Assert.Equal("second", result);
    }

    [Fact]
    public void GetWeighted_WhenArgumentsInvalid_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RandomHelper.GetWeighted<string>(null!, new[] { 1d }));
        Assert.Throws<ArgumentException>(() => RandomHelper.GetWeighted(new[] { "a" }, new[] { 1d, 2d }));
        Assert.Throws<ArgumentException>(() => RandomHelper.GetWeighted(Array.Empty<string>(), Array.Empty<double>()));
        Assert.Throws<ArgumentException>(() => RandomHelper.GetWeighted(new[] { "a" }, new[] { -1d }));
        Assert.Throws<ArgumentException>(() => RandomHelper.GetWeighted(new[] { "a" }, new[] { 0d }));
    }
}
