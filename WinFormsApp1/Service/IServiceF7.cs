using System;
using System.Collections.Generic;
using System.Linq;
using TaxiManager.Structure;

namespace TaxiManager.Service
{
    public interface IServiceF7
    {
        public static IServiceF7 Instance => ServiceF7.Instance;
        
        List<FrequentPath> GetTopKFrequentPaths(int k, double minDistance);
    }

}
