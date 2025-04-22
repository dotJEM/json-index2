namespace DotJEM.Json.Index2.Management.Source;

/// <summary>
/// 
/// </summary>
public struct GenerationInfo
{
    /// <summary>
    /// 
    /// </summary>
    public long Current { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public long Latest { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="current"></param>
    /// <param name="latest"></param>
    public GenerationInfo(long current, long latest)
    {
        Current = current;
        Latest = latest;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static GenerationInfo operator +(GenerationInfo left, GenerationInfo right)
    {
        return new(left.Current + right.Current, left.Latest + right.Latest);
    }
}