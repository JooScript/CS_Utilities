namespace Utils.DS;

public static class Sequence
{
    public static IEnumerable<int> Range(int start, int stop, int step = 1)
    {
        if (step == 0)
            throw new ArgumentException("step cannot be zero");

        if (step > 0)
        {
            for (int i = start; i < stop; i += step)
                yield return i;
        }
        else
        {
            for (int i = start; i > stop; i += step)
                yield return i;
        }
    }

    public static IEnumerable<int> Range(int stop, int step = 1)
    {
        return Range(0, stop, step);
    }
}
