namespace DotJEM.Json.Index2.Management.Source;

public struct GenerationInfo
{
    public long Current { get; }
    public long Latest { get; }

    public GenerationInfo(long current, long latest)
    {
        Current = current;
        Latest = latest;
    }

    public static GenerationInfo operator +(GenerationInfo left, GenerationInfo right)
    {
        return new GenerationInfo(left.Current + right.Current, left.Latest + right.Latest);
    }
}