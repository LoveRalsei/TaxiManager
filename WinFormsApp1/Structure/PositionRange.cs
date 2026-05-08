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
        /// <summary>
        /// 获取这个范围包含的所有瓦片
        /// </summary>
        public readonly List<Tile> GetTiles(byte tileSize = 1)
        {
            List<Tile> tiles = [];
            Tile tileA = Min.GetTile(tileSize), tileB = Max.GetTile(tileSize);
            uint xA = tileA.X, yA = tileA.Y, xB = tileB.X, yB = tileB.Y;
            // 当最大点刚好在瓦片边缘时
            // tileB会额外包含一个长度的瓦片群
            // 需要考虑边缘情况，进行剔除
            var rangeB = tileB.Range;
            if (Max.X == rangeB.Min.X)
                xB--;
            if (Max.Y == rangeB.Min.Y)
                yB--;
            for (uint i = xA; i <= xB; i++) for (uint j = yA; j <= yB; j++)
                    tiles.Add(Tile.From(tileSize, i, j));
            return tiles;
        }
        public readonly RectLatLng ToGmap() => new(Min, Max - Min);
        public static PositionRange FromGmap(RectLatLng rect) => FromUnsort(rect.LocationTopLeft, rect.LocationRightBottom);
        public static implicit operator PositionRange(RectLatLng rect) => FromGmap(rect);
        public static implicit operator RectLatLng(PositionRange range) => range.ToGmap();
    }
}
