using GMap.NET;
using TaxiManager.Structure;

namespace TaxiManager.Service
{
    public interface IServiceF4
    {
        public static IServiceF4 Instance => ServiceF4.Instance;
        
        /// <summary>
        /// 获取密度变化的热力图
        /// </summary>
        /// <param name="tileSize"></param>
        /// <param name="viewArea"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public Dictionary<Tile, Color> GetDensityChange(byte tileSize, RectLatLng viewArea, DateTime time);
    }
}
