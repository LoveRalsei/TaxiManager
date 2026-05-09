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
        /// <param name="viewArea"></param>
        /// <param name="gmapSize"></param>
        /// <param name="time"></param>
        /// <param name="maxDensity"></param> 多少密度作为最高密度（红色）
        /// <returns></returns>
        public Dictionary<Tile, Color> GetDensityChange(RectLatLng viewArea, Size gmapSize, DateTime time, int maxDensity = 50);
    }
}
