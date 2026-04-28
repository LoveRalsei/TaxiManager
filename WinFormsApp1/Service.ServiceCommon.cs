using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public class ServiceCommon : IServiceCommon
    {
        public static readonly ServiceCommon Instance = new();

        DateTime IServiceCommon.GetMaxTime() => DataLoader.TimeMax;

        DateTime IServiceCommon.GetMinTime() => DataLoader.TimeMin;
    }
}
