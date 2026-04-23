using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    /// <summary>
    /// 渲染所有出租车轨迹时，选用的渲染模式
    /// </summary>
    public enum F1RenderMode
    {
        Tiles, Points
    }
    public interface IServiceF1
    {
        public static IServiceF1 Instance { get => ServiceF1.Instance; }
        /// <summary>
        /// 在渲染所有出租车轨迹时，应当选用的渲染模式
        /// </summary>
        public abstract F1RenderMode GetRenderMode(RectLatLng viewArea);
        public abstract List<PointLatLng> GetPoints(RectLatLng viewArea, DateTime time);
        public abstract List<(Tile tile, uint count)> GetTiles(RectLatLng viewArea, DateTime time);
        /// <summary>
        /// 返回为null时，说明该司机此时没有工作
        /// </summary>
        public abstract Position? GetDriverPosition(int driverId, DateTime time);
        public abstract List<MapRoute> GetDriverRoutes(int driverId);

    }
}
