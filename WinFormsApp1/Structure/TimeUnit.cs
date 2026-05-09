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
        public static int GetUnit(DateTime time)
            => time.Year * 4 * 24 * 31 * 12 + time.Month * 4 * 24 * 31 + time.Day * 4 * 24 + time.Hour * 4 + time.Minute / 15;
    }
}
