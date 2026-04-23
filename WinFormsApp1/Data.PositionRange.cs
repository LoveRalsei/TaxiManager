using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public readonly record struct PositionRange
    {
        public readonly Position Min, Max;
        public PositionRange(Position min, Position max)
        {
            this.Min = min;
            this.Max = max;
        }
        public static PositionRange From(Position min, Position max) => new(min, max);
        public static PositionRange FromUnsort(Position a, Position b) =>
            From(Position.From(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)), Position.From(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y)));
        public readonly bool IsIn(Position pos) =>
            (pos.X >= Min.X && pos.X < Max.X && pos.Y >= Min.Y && pos.Y < Max.Y);
        
        public readonly RectLatLng ToGmap() => new(Min, Max - Min);
        public static PositionRange FromGmap(RectLatLng rect) => FromUnsort(rect.LocationTopLeft, rect.LocationRightBottom);
        public static implicit operator PositionRange(RectLatLng rect) => FromGmap(rect);
        public static implicit operator RectLatLng(PositionRange range) => range.ToGmap();
    }
}
