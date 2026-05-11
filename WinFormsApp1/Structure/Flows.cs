using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.Structure
{
    public static class Flows
    {
        
        private static Task? _task;
        public static bool Loaded => _task?.IsCompleted ?? false;

        private static readonly Dictionary<int, Dictionary<Driver, Position>> _positions = [];

        private static readonly Dictionary<Driver, Position> _emptyMap = [];


        /// <summary>
        /// 获取从起点到终点直线经过的所有大小为1的瓦片
        /// </summary>
        /// <param name="ignoreArguments">0: 无忽略, 1: 忽略头, 2: 忽略尾, 3: 忽略头尾</param>
        private static List<Tile> GetTilesOnLine(byte tileSize, Position from, Position to, int ignoreArguments = 0)
        {
            var tiles = new List<Tile>();

            var fromTile = from.GetTile(tileSize);
            var toTile = to.GetTile(tileSize);
            
            int startX = (int)fromTile.X;
            int startY = (int)fromTile.Y;
            int endX = (int)toTile.X;
            int endY = (int)toTile.Y;
            
            int dx = Math.Abs(endX - startX);
            int dy = Math.Abs(endY - startY);
            int sx = (startX < endX) ? 1 : -1;
            int sy = (startY < endY) ? 1 : -1;
            int err = dx - dy;
            
            int x = startX, y = startY;

            while (true)
            {
                if (ignoreArguments is 0 or 2)
                    tiles.Add(Tile.From(tileSize, (uint)x, (uint)y));
                
                int e2 = 2 * err;
                if (e2 >= -dx)
                {
                    x += sx;
                    err -= dy;
                }
                if (e2 <= dy)
                {
                    err += dx;
                    y += sy;
                }
                
                if (ignoreArguments == 1)
                    tiles.Add(Tile.From(tileSize, (uint)x, (uint)y));
                
                if (x == endX && y == endY)
                {
                    if (ignoreArguments == 0)
                        tiles.Add(Tile.From(tileSize, (uint)x, (uint)y));
                    break;
                } 
                if (ignoreArguments == 3)
                {
                    tiles.Add(Tile.From(tileSize, (uint)x, (uint)y));
                }
            }

            return tiles;
        }
        
        
        public static void Initialize()
        {
            _task = DataLoader.ExecuteAfterLoaded(() =>
            {
                Console.WriteLine("Initializing Flows");
                Stopwatch sw = Stopwatch.StartNew();
                var drivers = DataLoader.Drivers;
                var startUnit = TimeUnit.GetUnit(DataLoader.TimeMin);
                var endUnit = TimeUnit.GetUnit(DataLoader.TimeMax);
                for (var unit = startUnit; unit < endUnit; unit++)
                {
                    var unitMap = new Dictionary<Driver, Position>();
                    _positions.Add(unit, unitMap);
                    var time = TimeUnit.GetTime(unit);
                    foreach (var driver in drivers)
                    {
                        var position = driver.GetPosition(time);
                        if (position != null)
                            unitMap.Add(driver, position.Value);
                    }
                }
                sw.Stop();
                Console.WriteLine($"Flows Initialized in {sw.ElapsedMilliseconds}ms");
            });
        }

        /// <summary>
        /// 获得某个时间点，从 from Tile 出发的所有流
        /// </summary>
        /// <param name="from"></param>
        /// <param name="timeUnit"></param>
        /// <returns></returns>
        public static Dictionary<Tile, float> GetFlowFrom(Tile from, int timeUnit)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据未完成预处理");
                _task?.Wait();
            }

            var tileSize = from.Size;
            var unitMap = _positions.GetValueOrDefault(timeUnit, _emptyMap);
            var unitNextMap = _positions.GetValueOrDefault(timeUnit + 1, _emptyMap);
            Dictionary<Driver, Position> driversFrom = [];
            Dictionary<Driver, Position> driversTo = [];
            
            Dictionary<Tile, float> flows = [];
            
            foreach (var driver in DataLoader.Drivers)
            {
                if (unitMap.TryGetValue(driver, out Position position))
                    if (position.GetTile(tileSize) == from)
                        driversFrom.Add(driver, position);
            }

            foreach (var entry in driversFrom)
            {
                var driver = entry.Key;
                if (unitNextMap.TryGetValue(driver, out Position toPos))
                {
                    if (Tile.From(tileSize, toPos) == from)
                        continue;
                    var fromPos = entry.Value;
                    var passed = GetTilesOnLine(tileSize, fromPos, toPos, 1);
                    if (passed.Count == 0)
                        continue;
                    var flowDensity = 1.0f / passed.Count;
                    foreach (var toTile in passed)
                    {
                        if (flows.TryGetValue(toTile, out var flow))
                            flows[toTile] += flowDensity;
                        else
                            flows.Add(toTile, flowDensity);
                    }
                    
                }
            }

            return flows;
        }

        /// <summary>
        /// 获得某个时间点，到 to Tile 的所有流
        /// </summary>
        /// <param name="to"></param>
        /// <param name="timeUnit"></param>
        /// <returns></returns>
        public static Dictionary<Tile, float> GetFlowTo(Tile to, int timeUnit)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据未完成预处理");
                _task?.Wait();
            }
            
            var tileSize = to.Size;
            var unitMap = _positions.GetValueOrDefault(timeUnit, _emptyMap);
            var unitNextMap = _positions.GetValueOrDefault(timeUnit + 1, _emptyMap);
            Dictionary<Driver, Position> driversFrom = [];
            Dictionary<Driver, Position> driversTo = [];
            
            Dictionary<Tile, float> flows = [];
            
            foreach (var driver in DataLoader.Drivers)
            {
                if (unitNextMap.TryGetValue(driver, out Position position))
                    if (position.GetTile(tileSize) == to)
                        driversTo.Add(driver, position);
            }

            foreach (var entry in driversTo)
            {
                var driver = entry.Key;
                if (unitMap.TryGetValue(driver, out Position fromPos))
                {
                    if (Tile.From(tileSize, fromPos) == to)
                        continue;
                    var toPos = entry.Value;
                    var passed = GetTilesOnLine(tileSize, fromPos, toPos, 2);
                    if (passed.Count == 0)
                        continue;
                    var flowDensity = 1.0f / passed.Count;
                    foreach (var fromTile in passed)
                    {
                        if (flows.TryGetValue(fromTile, out var flow))
                            flows[fromTile] += flowDensity;
                        else
                            flows.Add(fromTile, flowDensity);
                    }
                }
            }

            return flows;
        }
    }
}
