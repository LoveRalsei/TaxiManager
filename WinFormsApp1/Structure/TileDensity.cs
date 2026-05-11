using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.Structure
{
    public class TileDensity
    {
        private readonly static Dictionary<int, Dictionary<Tile, float>> _countMap = [];
        private static Task? _task;
        public static bool Loaded => _task?.IsCompleted ?? false;
        public static bool IsError => _task?.IsFaulted ?? false;
        public static Exception? Error => _task?.Exception;
        public static float MaxDensity { get; private set; }

        public static void Initialize()
        {
            _task = DataLoader.ExecuteAfterLoaded(() =>
            {
                Console.WriteLine("Initializing TileDensity");
                Stopwatch sw = Stopwatch.StartNew();
                var drivers = DataLoader.Drivers;
                foreach (var driver in drivers)
                {
                    HashSet<(Tile tile, int unit)> passed = [];
                    foreach (var node in driver.Nodes)
                    {
                        passed.Add((node.Position.GetTile(), TimeUnit.GetUnit(node.Time)));
                    }
                    var eachDensity = 1.0f / passed.Count;
                    foreach (var entry in passed)
                    {
                        if (!_countMap.TryGetValue(entry.unit, out var tileMap))
                        {
                            tileMap = [];
                            _countMap[entry.unit] = tileMap;
                        }
                        
                        if (!tileMap.TryGetValue(entry.tile, out float density))
                            density = 0;
                        density += eachDensity;
                        MaxDensity = Math.Max(MaxDensity, density);
                        tileMap[entry.tile] = density;
                    }
                }
                _countMap.TrimExcess();
                sw.Stop();
                Console.WriteLine($"TileDensity Initialized in {sw.ElapsedMilliseconds}ms");
            });
        }

        /// <summary>
        /// 返回瓦片和时间指定的时空段中，存在过的车辆数
        /// </summary>
        /// <param name="tiles">大小为1的瓦片列表</param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static float GetCount(List<Tile> tiles, DateTime time)
            => GetCount(tiles, TimeUnit.GetUnit(time));

        public static float GetCount(List<Tile> tiles, int timeUnit, Stopwatch? timeCounter = null)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据加载未完成，请稍等……");
                _task?.Wait();
            }
            timeCounter?.Start();
            float count = 0f;
            if (!_countMap.TryGetValue(timeUnit, out var tileMap))
                return 0;
            foreach (var t in tiles)
            {
                if (tileMap.TryGetValue(t, out float density))
                    count += density;
            }
            timeCounter?.Stop();
            return count;
        }
    }
}
