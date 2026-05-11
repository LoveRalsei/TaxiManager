using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.Structure
{
    public class TimeUnit
    {
        public static readonly TimeSpan UnitTime = TimeSpan.FromMinutes(15);
        public static DateTime GetPrevUnitTime(DateTime time)
            => time - UnitTime;
        public static int GetPrevUnit(int unit)
            => unit - 1;
        public static int GetNextUnit(int unit)
            => unit + 1;
        public static int GetUnit(DateTime time)
            => (int)(time.Ticks / UnitTime.Ticks);

        public static DateTime GetTime(int unit)
            => new DateTime(unit * UnitTime.Ticks);
    }
}
