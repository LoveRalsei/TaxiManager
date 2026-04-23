using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public readonly struct TimeTolerance(long tolerance)
    {
        // 15 minutes tolerance by default.
        public const long Default = 1000 * 60 * 15;
        public readonly long MillisecondsTolerance = tolerance;
        public TimeTolerance() : this(Default) { }
        public static TimeTolerance From(long tolerance) => new(tolerance);
        public static TimeTolerance Seconds(long tolerance) => From(tolerance * 1000);
        public static TimeTolerance Minutes(long tolerance) => Seconds(60 * tolerance);
        public static TimeTolerance Hours(long tolerance) => Minutes(60 * tolerance);
        public static TimeTolerance Days(long tolerance) => Hours(24 * tolerance);
        public override string ToString() => $"{MillisecondsTolerance}ms";
        public static implicit operator long(TimeTolerance tolerance) => tolerance.MillisecondsTolerance;
        public static implicit operator TimeTolerance(long tolerance) => From(tolerance);
    };
}
