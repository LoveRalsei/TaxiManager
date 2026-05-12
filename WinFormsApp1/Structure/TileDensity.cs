using System.Diagnostics;

namespace TaxiManager.Structure
{
    public class TileDensity
    {
        private readonly static Dictionary<int, Dictionary<Tile, float>> _countMap = [];
        private readonly static HashSet<Tile> _emptyTiles = [];
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
                HashSet<Tile> existTiles = [];
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
                        existTiles.Add(entry.tile);
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
                foreach (var tile in Tile.GetAllTiles().Where(tile => !existTiles.Contains(tile)))
                {
                    _emptyTiles.Add(tile);
                }
                sw.Stop();
                Console.WriteLine($"TileDensity Initialized in {sw.ElapsedMilliseconds}ms");
            });
        }

        public static Dictionary<Tile, float> GetDensity(byte tileSize, int timeUnit, Stopwatch? timeCounter = null)
        {
            if (tileSize == 1)
                return GetDensity(timeUnit, timeCounter);
            if (!Loaded)
            {
                MessageBox.Show("数据加载未完成，请稍等……");
                _task?.Wait();
            }
            timeCounter?.Start();
            Dictionary<Tile, float> densityMap = [];
            if (!_countMap.TryGetValue(timeUnit, out var tileMap))
                return densityMap;
            foreach (var (t, density) in tileMap)
            {
                var tile = t.ToSize(tileSize);
                densityMap[tile] = densityMap.GetValueOrDefault(tile, 0) + density;
            }
            timeCounter?.Stop();
            return densityMap;
        }

        public static Dictionary<Tile, float> GetDensity(int timeUnit, Stopwatch? timeCounter = null)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据加载未完成，请稍等……");
                _task?.Wait();
            }

            return _countMap.GetValueOrDefault(timeUnit, []);
        }

        public static bool IsEmpty(Tile tile)
            => tile.Size == 1 ? _emptyTiles.Contains(tile) : tile.SubTiles.All(IsEmpty);
    }
}
