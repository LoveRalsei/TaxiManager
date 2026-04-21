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
    public readonly struct PositionRaw
    {
        public readonly double Longitude;
        public readonly double Latitude;
        private PositionRaw(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
        public static PositionRaw Make(double longitude, double latitude)
        {
            return new PositionRaw(longitude, latitude);
        }
    }
}
