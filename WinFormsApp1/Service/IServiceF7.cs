using System;
using System.Collections.Generic;
using System.Linq;
using TaxiManager.Structure;

namespace TaxiManager.Service
{
    /// <summary>
    /// 频繁路径分析结果
    /// </summary>
    public class FrequentPathResult
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

    public interface IServiceF7
    {
        public static IServiceF7 Instance => ServiceF7.Instance;
        
        /// <summary>
        /// 获取整个城市中最频繁的前k条路径
        /// 基于TileDensity预计算的流量数据，高效统计路径频繁度
        /// </summary>
        /// <param name="k">返回的路径数量</param>
        /// <param name="minLengthMeters">最小路径长度（米）</param>
        /// <param name="sampleInterval">采样间隔（时间单位），默认60表示每小时采样一次</param>
        /// <returns>按频率降序排列的频繁路径列表</returns>
        List<FrequentPathResult> GetTopKFrequentPaths(int k, double minLengthMeters, int sampleInterval = 60);
    }
}
