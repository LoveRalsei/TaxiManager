using GMap.NET;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.Structure
{
    /// <summary>
    /// 以1米（等价于1e5经纬度）为一单位的坐标
    /// </summary>
    public readonly record struct Position(int X, int Y)
    {
        public const double MinLongitude = 115.3, MaxLongitude = 117.6, MinLatitude = 39.4, MaxLatitude = 41.1;
        public const int MinX = (int)(MinLongitude * 1e5), MaxX = (int)(MaxLongitude * 1e5), 
            MinY = (int)(MinLatitude * 1e5), MaxY = (int)(MaxLatitude * 1e5);
        public static readonly Position Min = new(MinX, MinY);
        public static readonly Position Max = new(MaxX, MaxY);
        public readonly int X = X;
        public readonly int Y = Y;

        public Position? Lerp(Position? target, float scale)
        {
            return Lerp(this, target, scale);
        }
        public bool IsValid() => IsValid(this);
        public static bool IsValid(Position? position) => position != null && IsValid(position.Value.X, position.Value.Y);
        public static bool IsValid(double longitude, double latitude) => IsValid(FromRaw(longitude, latitude));
        public static bool IsValid(int x, int y) => x is >= MinX and <= MaxX && y is >= MinY and <= MaxY;
        public static Position? Lerp(Position? from, Position? to, float scale)
        {
            if (from == null && to == null) return null;
            if (from == null) return to;
            if (to == null) return from;
            // 避免下溢
            double dx = (double)from.Value.X + ((double)to.Value.X - (double)from.Value.X) * scale;
            double dy = (double)from.Value.Y + ((double)to.Value.Y - (double)from.Value.Y) * scale;
            // 四舍五入并转换为uint
            int x = (int)Math.Round(dx);
            int y = (int)Math.Round(dy);
            // 确保结果在有效范围内
            if (x < MinX) x = MinX;
            if (x > MaxX) x = MaxX;
            if (y < MinY) y = MinY;
            if (y > MaxY) y = MaxY;
            return From(x, y);
        }
        public static Position FromRaw(double longitude, double latitude) => new((int)Math.Round(longitude * 1e5), (int)Math.Round(latitude * 1e5));
        public static Position FromRaw(PositionRaw raw) => FromRaw(raw.Longitude, raw.Latitude);
        public static Position FromGmap(PointLatLng point) => FromRaw(point.Lng, point.Lat);
        public static Position From(int x, int y) => new(x, y);
        /// <summary>
        /// 转换成经纬度格式
        /// </summary>
        public PositionRaw ToRaw() => PositionRaw.From(X*1e-5, Y*1e-5);
        public PointLatLng ToGmap() => ToRaw().ToGmap();

        public Position ToValid()
        {
            var x = X;
            if (x < MinX) x = MinX;
            if (x > MaxX) x = MaxX;
            var y = Y;
            if (y < MinY) y = MinY;
            if (y > MaxY) y = MaxY;
            return From(x, y);
        }
        public Tile GetTile(byte size = 1) => Tile.From(size, this);
        public double DistanceSquaredTo(Position? target)
        {
            if (target == null) return 0;
            var dx = X - target.Value.X;
            var dy = Y - target.Value.Y;
            return dx * dx + dy * dy;
        }
        public double DistanceTo(Position? target)
            => target == null ? 0 : Math.Sqrt(DistanceSquaredTo(target));
        public static implicit operator Position(PositionRaw raw) => FromRaw(raw);
        public static implicit operator Position(PointLatLng point) => FromGmap(point);
        public static implicit operator PointLatLng(Position position) => position.ToGmap();
        public static Position operator +(Position position, SizeLatLng offset) => 
            (position.ToRaw() + offset).ToMeter();
        public static Position operator -(Position position, SizeLatLng offset) => 
            (position.ToRaw() - offset).ToMeter();
        public static SizeLatLng operator -(Position a, Position b) => a.ToRaw() - b.ToRaw();

        public override string ToString()
        {
            return $"[{X},{Y}]";
        }
    }
}
