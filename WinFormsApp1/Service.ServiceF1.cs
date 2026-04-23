using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public class ServiceF1 : IServiceF1
    {
        public static readonly ServiceF1 Instance = new();
        Position? IServiceF1.GetDriverPosition(int driverId, DateTime time)
        {
            if (driverId <= 0 || driverId > DataLoader.DriversCount)
                throw new ArgumentOutOfRangeException(nameof(driverId));
            return DataLoader.Drivers[driverId-1].GetPosition(time);
        }
        List<MapRoute> IServiceF1.GetDriverRoutes(int driverId)
        {
            if (driverId <= 0 || driverId > DataLoader.DriversCount)
                throw new ArgumentOutOfRangeException(nameof(driverId));
            return DataLoader.Drivers[driverId - 1].GetRoutes();
        }

        F1RenderMode IServiceF1.GetRenderMode(RectLatLng viewArea)
        {
            Rectangle? screen = (Screen.PrimaryScreen?.WorkingArea) ?? throw new Exception("Without window(form)!");
            double meterWidthPixel = viewArea.WidthLng * 1e5 / screen.Value.Width;
            double meterHeightPixel = viewArea.HeightLat * 1e5 / screen.Value.Height;
            if (meterWidthPixel >= 10 || meterHeightPixel >= 10)
                return F1RenderMode.Tiles;
            return F1RenderMode.Points;
        }

        List<PointLatLng> IServiceF1.GetPoints(RectLatLng viewArea, DateTime time)
        {
            PositionRange range = viewArea;
            List<PointLatLng> points = [];
            foreach (var driver in DataLoader.Drivers)
            {
                var position = driver.GetPosition(time);
                if (position != null && range.IsIn(position.Value))
                    points.Add(position.Value);
            }
            return points;
        }

        private byte GetTileSize(RectLatLng viewArea)
        {
            Rectangle? screen = (Screen.PrimaryScreen?.WorkingArea) ?? throw new Exception("Without window(form)!");
            double meterWidthPixel = viewArea.WidthLng * 1e5 / screen.Value.Width;
            double meterHeightPixel = viewArea.HeightLat * 1e5 / screen.Value.Height;
            return (byte)Math.Min(255, Math.Min(meterHeightPixel, meterWidthPixel) * 10 / 100);
        }

        List<(Tile tile, uint count)> IServiceF1.GetTiles(RectLatLng viewArea, DateTime time)
        {
            PositionRange range = viewArea;
            var tileSize = GetTileSize(viewArea);
            Dictionary<Tile, uint> driverCount = [];
            foreach (var driver in DataLoader.Drivers)
            {
                var position = driver.GetPosition(time);
                if (position == null || !range.IsIn(position.Value)) 
                    continue;
                var posTile = Tile.From(tileSize, position.Value);
                if (driverCount.TryGetValue(posTile, out uint value))
                    driverCount[posTile] = ++value;
                else
                    driverCount[posTile] = 1;
            }
            List<(Tile tile, uint count)> list = new(driverCount.Count);
            foreach (var pair in driverCount) list.Add((pair.Key, pair.Value));
            return list;
        }
    }
}
