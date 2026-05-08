using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiManager.Structure;

namespace TaxiManager.Service
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
            return DataLoader.Drivers[driverId - 1].GetRoutes(TimeTolerance.Minutes(15));
        }

        F1RenderMode IServiceF1.GetRenderMode(RectLatLng viewArea, Size gmapSize)
        {
            double meterWidthPixel = viewArea.WidthLng * 1e5 / gmapSize.Width;
            double meterHeightPixel = viewArea.HeightLat * 1e5 / gmapSize.Height;
            if (meterWidthPixel >= 10 && meterHeightPixel >= 10)
                return F1RenderMode.Tiles;
            return F1RenderMode.Points;
        }

        List<PointLatLng> IServiceF1.GetPoints(RectLatLng viewArea, Size gmapSize, DateTime time)
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

        private byte GetTileSize(RectLatLng viewArea, Size gmapSize)
        {
            Rectangle? screen = (Screen.PrimaryScreen?.WorkingArea) ?? throw new Exception("Without window(form)!");
            double meterWidthPixel = viewArea.WidthLng * 1e5 / gmapSize.Width;
            double meterHeightPixel = viewArea.HeightLat * 1e5 / gmapSize.Height;
            return (byte)Math.Min(255, Math.Min(meterHeightPixel, meterWidthPixel) * 15 / 100);
        }

        List<(Tile tile, uint count)> IServiceF1.GetTiles(RectLatLng viewArea, Size gmapSize, DateTime time)
        {
            PositionRange range = viewArea;
            var tileSize = GetTileSize(viewArea, gmapSize);
            Dictionary<Tile, uint> driverCount = [];
            foreach (var driver in DataLoader.Drivers)
            {
                var position = driver.GetPosition(time);
                if (position == null || !range.IsIn(position.Value)) 
                    continue;
                var posTile = Tile.From(tileSize, position.Value);
                if (driverCount.TryGetValue(posTile, out uint value))
                    driverCount[posTile] = value + 1;
                else
                    driverCount[posTile] = 1;
            }
            List<(Tile tile, uint count)> list = new(driverCount.Count);
            foreach (var pair in driverCount) list.Add((pair.Key, pair.Value));
            return list;
        }
    }
}
