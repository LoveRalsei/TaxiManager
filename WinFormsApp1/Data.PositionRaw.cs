using GMap.NET;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    /// <summary>
    /// 经纬度坐标
    /// </summary>
    public readonly record struct PositionRaw
    {
        public readonly double Longitude;
        public readonly double Latitude;
        public PositionRaw(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
        public static PositionRaw From(double longitude, double latitude) => new(longitude, latitude);
        public static PositionRaw FromMeter(Position position) => position.ToRaw();
        public static PositionRaw FromGmap(PointLatLng point) => From(point.Lng, point.Lat);
        /// <summary>
        /// 转换为米单位的格式
        /// </summary>
        public Position ToMeter() => Position.FromRaw(this);
        public PointLatLng ToGmap() => new(Latitude, Longitude);
        public static implicit operator PositionRaw(Position meter) => FromMeter(meter);
        public static implicit operator PositionRaw(PointLatLng point) => FromGmap(point);
        public static implicit operator PointLatLng(PositionRaw raw) => raw.ToGmap();
        public static PositionRaw operator +(PositionRaw raw, SizeLatLng offset) => 
            From(raw.Longitude + offset.WidthLng, raw.Latitude + offset.HeightLat);
        public static PositionRaw operator -(PositionRaw raw, SizeLatLng offset) =>
            From(raw.Longitude - offset.WidthLng, raw.Latitude - offset.HeightLat);
        public static SizeLatLng operator -(PositionRaw a, PositionRaw b) => 
            new(a.Latitude - b.Latitude, a.Longitude - b.Longitude);
    }
}
