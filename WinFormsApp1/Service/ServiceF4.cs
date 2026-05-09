using GMap.NET;
using TaxiManager.Structure;

namespace TaxiManager.Service
{
    public class ServiceF4 : IServiceF4
    {
        public static readonly ServiceF4 Instance = new();
        
        private byte GetTileSize(RectLatLng viewArea, Size gmapSize)
        {
            double meterWidthPixel = viewArea.WidthLng * 1e5 / gmapSize.Width;
            double meterHeightPixel = viewArea.HeightLat * 1e5 / gmapSize.Height;
            return (byte)Math.Max(1, Math.Min(255, Math.Min(meterHeightPixel, meterWidthPixel) * 15 / 100));
        }

        /// <summary>
        /// 获取密度变化的热力图
        /// </summary>
        /// <param name="viewArea"></param>
        /// <param name="gmapSize"></param>
        /// <param name="time"></param>
        /// <param name="maxDensity"></param> 多少密度作为最高密度（红色）
        /// <returns></returns>
        Dictionary<Tile, Color> IServiceF4.GetDensityChange(RectLatLng viewArea, Size gmapSize, DateTime time, int maxDensity = 50)
        {
            var tileSize = GetTileSize(viewArea, gmapSize);
            Dictionary<Tile, Color> map = [];
            var tiles = PositionRange.FromGmap(viewArea).GetTiles(tileSize);
            foreach (var tile in tiles)
            {
                var smallTiles = tile.SubTiles;
                var currDensity = TileDensity.GetCount(smallTiles, time);
                var prevDensity = TileDensity.GetCount(smallTiles, TileDensity.GetPrevUnitTime(time));
                double densityChange = currDensity - prevDensity;
                densityChange /= (double)maxDensity;
                Color? color = null;
                if (densityChange >= 1)
                    color = Color.FromArgb(0x7fff0000);
                else if (densityChange >= 0.5)
                    color = Color.FromArgb(0x7fff0000 | (((int)(0xff * (2 - 2 * densityChange))) << 2));
                else if (densityChange > 0)
                    color = Color.FromArgb(0x7f00ff00 | (((int)(0xff * 2 * densityChange)) << 4));
                if (color != null)
                    map.Add(tile, color.Value);
            }
            return map;
        }
    }
}
