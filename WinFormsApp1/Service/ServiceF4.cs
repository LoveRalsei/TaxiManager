using System.Diagnostics;
using GMap.NET;
using TaxiManager.Structure;

namespace TaxiManager.Service
{
    public class ServiceF4 : IServiceF4
    {
        public static readonly ServiceF4 Instance = new();

        /// <summary>
        /// 获取密度变化的热力图
        /// </summary>
        /// <param name="tileSize"></param>
        /// <param name="viewArea"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        Dictionary<Tile, Color> IServiceF4.GetDensityChange(byte tileSize, RectLatLng viewArea, DateTime time)
        {
            var service = IServiceCommon.Instance;
            
            var viewRange = PositionRange.FromGmap(viewArea);
            var unitFrom = TimeUnit.GetUnit(time);
            var unitTo = unitFrom + 3;
            var densityChanges =
                Density.GetDensityChange(tileSize, unitFrom, unitTo, (tile, f) => viewRange.IsIn(tile.Index));

            var maxDensity = Density.MaxDensity / 3;
            return densityChanges.Select(pair => (pair.Key, service.GetHotColor(pair.Value / maxDensity, 0.2f))).ToDictionary();
        }
    }
}
