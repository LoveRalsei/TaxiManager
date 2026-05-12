using System.Diagnostics;
using GMap.NET;
using TaxiManager.Structure;

namespace TaxiManager.Service
{
    public class ServiceF4 : IServiceF4
    {
        public static readonly ServiceF4 Instance = new();
        
        private byte GetTileSize(RectLatLng viewArea, Size gmapSize)
        {
            if (gmapSize.Width == 0 || gmapSize.Height == 0)
                return 1;
            double meterWidthPixel = viewArea.WidthLng * 1e5 / gmapSize.Width;
            double meterHeightPixel = viewArea.HeightLat * 1e5 / gmapSize.Height;
            return (byte)Math.Max(1, Math.Min(255, Math.Min(meterHeightPixel, meterWidthPixel) * 15 / 100));
        }

        /// <summary>
        /// 获取密度变化的热力图
        /// </summary>
        /// <param name="tileSize"></param>
        /// <param name="viewArea"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        Dictionary<Tile, Color> IServiceF4.GetDensityChange(byte tileSize, RectLatLng viewArea, DateTime time)
        {
            const float maxDensity = 0.02f;
            var service = IServiceCommon.Instance;
            Dictionary<Tile, Color> map = [];
            
            var viewRange = PositionRange.FromGmap(viewArea);
            var unit = TimeUnit.GetUnit(time);
            var nextUnit = TimeUnit.GetNextUnit(unit);
            var densityMap = TileDensity.GetDensity(tileSize, unit);
            var densityMapNext = TileDensity.GetDensity(tileSize, nextUnit);
            foreach (var (tile, density) in densityMap)
            {
                if (!viewRange.IsIn(tile.Index)) continue;
                if (!densityMapNext.TryGetValue(tile, out var densityNext)) continue;
                var densityChange = Math.Abs(density - densityNext);
                densityChange /= maxDensity;
                if (densityChange > 0)
                    map.Add(tile, service.GetHotColor(densityChange, 0.2f));
            }
            return map;
        }
    }
}
