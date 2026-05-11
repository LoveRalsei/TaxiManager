using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.Structure
{
    public readonly record struct PositionRange
    {
        public readonly Position Min, Max;
        public List<Position> Corners => [Min, Position.From(Max.X, Min.Y), Max, Position.From(Min.X, Max.Y)];
        public PositionRange(Position min, Position max)
        {
            Min = min;
            Max = max;
        }
        public static PositionRange From(Position min, Position max) => new(min, max);
        public static PositionRange FromUnsort(Position a, Position b) =>
            From(Position.From(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)), Position.From(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y)));
        public readonly bool IsIn(Position pos) =>
            pos.X >= Min.X && pos.X < Max.X && pos.Y >= Min.Y && pos.Y < Max.Y;
        public bool IsIn(PositionRange other) => IsIn(other.Min) && IsIn(other.Max);
        public bool IsIntersect(PositionRange other)
        {
            var corners = Corners;
            foreach (var corner in corners)
                if (other.IsIn(corner))
                    return true;
            return false;
        }
        /// <summary>
        /// 获取这个范围包含的所有瓦片
        /// </summary>
        public readonly List<Tile> GetTiles(byte tileSize = 1)
            => Tile.GetTilesIn(tileSize, this);
        public readonly RectLatLng ToGmap() => new(Min, Max - Min);
        public readonly PositionRange ToValid() => new PositionRange(Min.ToValid(), Max.ToValid());
        public static PositionRange FromGmap(RectLatLng rect) => FromUnsort(rect.LocationTopLeft, rect.LocationRightBottom);
        public static implicit operator PositionRange(RectLatLng rect) => FromGmap(rect);
        public static implicit operator RectLatLng(PositionRange range) => range.ToGmap();
    }
}
