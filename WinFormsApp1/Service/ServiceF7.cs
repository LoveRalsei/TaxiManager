using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaxiManager.Structure;

namespace TaxiManager.Service
{
    public class ServiceF7 : IServiceF7
    {
        public static readonly ServiceF7 Instance = new();
        
        /// <summary>
        /// 获取前K个频繁路径
        /// </summary>
        /// <param name="k">返回的路径数量</param>
        /// <param name="minDistance">最小长度</param>
        /// <returns>按频率排序的前K个频繁路径列表</returns>
        List<FrequentPath> IServiceF7.GetTopKFrequentPaths(int k, double minDistance)
        {
            // 从预处理的频繁路径中获取前K个
            var allPaths = Paths.FrequentPaths;
            return allPaths.Where(path => path.LengthMeters >= minDistance).Take(k).ToList();
        }
    }
}
