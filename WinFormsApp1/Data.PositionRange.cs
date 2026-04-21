using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public readonly record struct PositionRange
    {
        public readonly Position min, max;
        private PositionRange(Position min, Position max)
        {
            this.min = min;
            this.max = max;
        }
        public static PositionRange Make(Position min, Position max)
        {
            return new(min, max);
        }
        public static PositionRange MakeAuto(Position a, Position b)
        {
            return Make(Position.Make(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)), Position.Make(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y)));
        }
        public readonly bool IsIn(Position pos)
        {
            return (pos.X >= min.X && pos.X < max.X && pos.Y >= min.Y && pos.Y < max.Y);
        }
    }
}
