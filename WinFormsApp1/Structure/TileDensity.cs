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

        public static void Initialize()
        {
            _task = DataLoader.ExecuteAfterLoaded(() =>
            {
                var drivers = DataLoader.Drivers;
                foreach (var driver in drivers)
                {
                    HashSet<(Tile tile, int unit)> passed = [];
                    foreach (var node in driver.Nodes)
                    {
                        passed.Add((node.Position.GetTile(), TimeUnit.GetUnit(node.Time)));
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
        /// <param name="tiles">大小为1的瓦片列表</param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int GetCount(List<Tile> tiles, DateTime time)
            => GetCount(tiles, TimeUnit.GetUnit(time));

        public static int GetCount(List<Tile> tiles, int timeUnit)
        {
            if (!Loaded)
                MessageBox.Show("数据加载未完成，请稍等……");
            _task?.Wait();
            int count = 0;
            foreach (var t in tiles)
            {
                if (_countMap.TryGetValue((t, timeUnit), out int tileCount))
                    count += tileCount;
            }
            return count;
        }
    }
}
