using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaxiManager.Structure;

namespace TaxiManager.Service;

public class ServiceF8 : IServiceF8
{
    public static readonly ServiceF8 Instance = new();
    
    /// <summary>
    /// 获取从区域A到区域B的前K个频繁路径
    /// </summary>
    List<FrequentPath> IServiceF8.GetTopKFrequentPaths(PositionRange regionA, PositionRange regionB, int k)
        => Paths.ExtractFrequentPaths(Paths.FlowsTotal, regionA, regionB).Take(k).ToList();
    
}
