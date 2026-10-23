namespace Utils.Bit;

public static class BitHelper
{
    /// <summary>
    /// Combines multiple bit flags into a single integer value.
    /// </summary>
    public static int Combine(params int[] values)
    {
        return values.Aggregate(
            0,
            (current, value) => current | value);
    }

    /// <summary>
    /// Determines whether the specified value contains the required bit flags.
    /// </summary>
    public static bool Has(int required, int value)
    {
        return (value & required) == required;
    }
}
