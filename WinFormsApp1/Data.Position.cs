using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    /// <summary>
    /// 以1米（等价于1e5经纬度）为一单位的坐标
    /// </summary>
    public readonly record struct Position
    {
        public const double MinLongitude = 115.3, MaxLongitude = 117.6, MinLatitude = 39.4, MaxLatitude = 41.1;
        public const uint MinX = 11530000, MaxX = 11760000, MinY = 3940000, MaxY = 4110000;
        public readonly uint X;
        public readonly uint Y;
        private Position(uint x, uint y) : this() { X = x; Y = y; }
        private Position(double longitude, double latitude) : this((uint)Math.Round(longitude * 1e5), (uint)Math.Round(latitude * 1e5)) { }
        public Position? Lerp(Position? target, float scale)
        {
            return Lerp(this, target, scale);
        }
        public bool IsValid() => IsValid(this);
        public static bool IsValid(Position? position) => position != null && IsValid(position.Value.X, position.Value.Y);
        public static bool IsValid(double longitude, double latitude) => IsValid(MakeFromRaw(longitude, latitude));
        public static bool IsValid(uint x, uint y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
        public static Position? Lerp(Position? from, Position? to, float scale)
        {
            if (from == null && to == null) return null;
            if (from == null) return to;
            if (to == null) return from;
            return Position.MakeFromRaw(
                from.Value.X + (to.Value.X - from.Value.X) * scale,
                from.Value.Y + (to.Value.Y - from.Value.Y) * scale
                );
        }
        public static Position MakeFromRaw(double longitude, double latitude) => new(longitude, latitude);
        public static Position MakeFromRaw(PositionRaw raw) => MakeFromRaw(raw.Longitude, raw.Latitude);
        public static Position Make(uint x, uint y) => new(x, y);
        public PositionRaw ToRaw() => PositionRaw.Make(X*1e-5, Y*1e-5);
        public Tile GetTile(byte size = 1)
        {
            return Tile.Make(size, this);
        }
    }
}
