namespace TaxiManager.Structure;

public class FrequentPath
{
    /// <summary>
    /// 路径的瓦片序列
    /// </summary>
    public List<Tile> PathTiles { get; set; } = new();
        
    /// <summary>
    /// 路径的频繁度（通行的汽车总数）
    /// </summary>
    public float Frequency { get; set; }
        
    /// <summary>
    /// 路径长度（米）
    /// </summary>
    public double LengthMeters { get; set; }
}