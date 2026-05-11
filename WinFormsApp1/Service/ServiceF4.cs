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
            Dictionary<Tile, Color> map = [];
            var tiles = PositionRange.FromGmap(viewArea).GetTiles(tileSize);
            var unit = TimeUnit.GetUnit(time);
            var prevUnit = TimeUnit.GetPrevUnit(unit);
            foreach (var tile in tiles)
            {
                if (TileDensity.IsEmpty(tile))
                    continue;
                var smallTiles = tile.SubTiles;
                var currDensity = TileDensity.GetCount(smallTiles, unit);
                var prevDensity = TileDensity.GetCount(smallTiles, prevUnit);
                double densityChange = Math.Abs(currDensity - prevDensity);
                densityChange /= maxDensity;
                Color? color = null;
                if (densityChange >= 1)
                    color = Color.FromArgb(0x7fdf0000);
                else if (densityChange >= 0.2)
                    color = Color.FromArgb(0x7fdf0000 | (((int)(0xdf * 0.25 * (5 - 5 * densityChange))) << 8));
                else if (densityChange > 0)
                    color = Color.FromArgb(0x7f00df00 | (((int)(0xdf * 5 * densityChange)) << 16));
                if (color != null)
                    map.Add(tile, color.Value);
            }
            return map;
        }
    }
}
