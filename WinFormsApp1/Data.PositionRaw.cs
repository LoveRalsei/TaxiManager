using System;
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
        private PositionRaw(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
        public static PositionRaw Make(double longitude, double latitude) => new(longitude, latitude);
        public static PositionRaw MakeFromMeter(Position position) => position.ToRaw();
        /// <summary>
        /// 转换为米单位的格式
        /// </summary>
        public Position ToMeter()
        {
            return Position.MakeFromRaw(this);
        }
    }
}
