using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public interface IServiceCommon
    {
        public static IServiceCommon Instance => ServiceCommon.Instance;
        public abstract DateTime GetMinTime();
        public abstract DateTime GetMaxTime();
    }
}
