using System.Collections.Generic;
using TaxiManager.Structure;

namespace TaxiManager.Service;

public interface IServiceF8
{
    public static IServiceF8 Instance => ServiceF8.Instance;
    
    /// <summary>
    /// 获取从区域A到区域B的前K个频繁路径
    /// </summary>
    /// <param name="regionA">起点区域</param>
    /// <param name="regionB">终点区域</param>
    /// <param name="k">返回的路径数量</param>
    /// <returns>按频率排序的前K个OD频繁路径列表</returns>
    List<FrequentPath> GetTopKFrequentPaths(PositionRange regionA, PositionRange regionB, int k);
}
