using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.Structure
{
    public class TileDensity
    {
        private readonly static Dictionary<(Tile tile, int unit), int> _countMap = [];
        private static Task? _task;
        public static bool Loaded => _task?.IsCompleted ?? false;
        public static bool IsError => _task?.IsFaulted ?? false;
        public static Exception? Error => _task?.Exception;

        public static readonly TimeSpan UnitTime = TimeSpan.FromMinutes(15);
        public static DateTime GetPrevUnitTime(DateTime time)
            => time - UnitTime;
        public static int GetUnit(DateTime time)
            => time.Year * 4 * 24 * 31 * 12 + time.Month * 4 * 24 * 31 + time.Day * 4 * 24 + time.Hour * 4 + time.Minute / 15;
        public static void Initialize()
        {
            _task = Task.Run(() =>
            {
                var drivers = DataLoader.Drivers;
                foreach (var driver in drivers)
                {
                    HashSet<(Tile tile, int unit)> passed = [];
                    foreach (var node in driver.Nodes)
                    {
                        passed.Add((node.Position.GetTile(), GetUnit(node.Time)));
                    }
                    foreach (var entry in passed)
                    {
                        if (_countMap.TryGetValue(entry, out int density))
                            _countMap[entry] = density + 1;
                        else 
                            _countMap[entry] = 1;
                    }
                }
            });
        }
        /// <summary>
        /// 返回瓦片和时间指定的时空段中，存在过的车辆数
        /// </summary>
        public static int GetCount(Tile tile, DateTime time) => GetCount(tile.SubTiles, time);

        /// <summary>
        /// 返回瓦片和时间指定的时空段中，存在过的车辆数
        /// </summary>
        public static int GetCount(List<Tile> tiles, DateTime time)
        {
            _task?.Wait();
            int unit = GetUnit(time);
            int count = 0;
            foreach (var t in tiles)
            {
                if (_countMap.TryGetValue((t, unit), out int tileCount))
                    count += tileCount;
            }
            return count;
        }
    }
}
