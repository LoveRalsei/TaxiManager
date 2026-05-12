using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiManager.Service;

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

        private static bool MaybeCross(Position from, Position to, PositionRange range)
        {
            uint fromX = from.X, fromY = from.Y, toX = to.X, toY = to.Y;
            var min = range.Min;
            var max = range.Max;
            if (Math.Max(fromX, toX) < min.X)
                return false;
            if (Math.Max(fromY, toY) < min.Y)
                return false;
            if (Math.Min(fromX, toX) > max.X)
                return false;
            if (Math.Min(fromY, toY) > max.Y)
                return false;
            return true;
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
                            unitMap[driver] = position.Value;
                    }
                }
                sw.Stop();
                Console.WriteLine($"Flows Initialized in {sw.ElapsedMilliseconds}ms");
            });
        }

        /// <summary>
        /// 获得某个时间点，从 from Tile 出发的所有流
        /// 需要注意的是，这些流包括了路过的瓦片的值，而不只是作为终点的值
        /// 其流量会比 GetFlow(rangeA, rangeB, timeUnit) 更大
        /// </summary>
        /// <param name="from"></param>
        /// <param name="timeUnit"></param>
        /// <returns></returns>
        public static Dictionary<Tile, float> GetFlowFrom(PositionRange range, int timeUnit)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据未完成预处理");
                _task?.Wait();
            }

            byte tileSize = 1;
            var unitMap = _positions.GetValueOrDefault(timeUnit, _emptyMap);
            var unitNextMap = _positions.GetValueOrDefault(timeUnit + 1, _emptyMap);
            
            Dictionary<Driver, Position> driversFrom = [];
            Dictionary<Tile, float> flows = [];
            
            foreach (var driver in DataLoader.Drivers)
            {
                if (!unitMap.TryGetValue(driver, out var position)) continue;
                if (range.IsIn(position))
                {
                    driversFrom.Add(driver, position);
                }
            }
            
            foreach (var entry in driversFrom)
            {
                var driver = entry.Key;
                if (!unitNextMap.TryGetValue(driver, out var toPos)) continue;
                if (range.IsIn(toPos)) continue;
                
                var fromPos = entry.Value;
                var passed = GetTilesOnLine(tileSize, fromPos, toPos, 1);
                if (passed.Count == 0) continue;
                
                var flowDensity = 1.0f / passed.Count;
                foreach (var toTile in passed)
                {
                    if (flows.TryGetValue(toTile, out var flow))
                        flows[toTile] += flowDensity;
                    else
                        flows.Add(toTile, flowDensity);
                }
            }

            return flows;
        }

        /// <summary>
        /// 获得某个时间点，到 to Tile 的所有流
        /// 需要注意的是，这些流包括了路过的瓦片的值，而不只是作为终点的值
        /// 其流量会比 GetFlow(rangeA, rangeB, timeUnit) 更大
        /// </summary>
        /// <param name="to"></param>
        /// <param name="timeUnit"></param>
        /// <returns></returns>
        public static Dictionary<Tile, float> GetFlowTo(PositionRange to, int timeUnit)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据未完成预处理");
                _task?.Wait();
            }
            
            byte tileSize = 1;
            var unitMap = _positions.GetValueOrDefault(timeUnit, _emptyMap);
            var unitNextMap = _positions.GetValueOrDefault(timeUnit + 1, _emptyMap);
            Dictionary<Driver, Position> driversTo = [];
            
            Dictionary<Tile, float> flows = [];
            
            foreach (var driver in DataLoader.Drivers)
            {
                if (!unitNextMap.TryGetValue(driver, out var position)) continue;
                if (to.IsIn(position))
                    driversTo.Add(driver, position);
            }

            foreach (var entry in driversTo)
            {
                var driver = entry.Key;
                if (!unitMap.TryGetValue(driver, out Position fromPos)) continue;
                if (to.IsIn(fromPos)) continue;
                
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

            return flows;
        }

        public static (float fromAtoB, float fromBtoA) GetFlowFromTo(PositionRange rangeA, PositionRange rangeB,
            int timeUnit)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据未完成预处理");
                _task?.Wait();
            }

            var service = IServiceCommon.Instance;
            
            var unitMap = _positions.GetValueOrDefault(timeUnit, _emptyMap);
            var unitNextMap = _positions.GetValueOrDefault(timeUnit + 1, _emptyMap);

            float fromAtoB = 0;
            float fromBtoA = 0;
            foreach (var driver in DataLoader.Drivers)
            {
                if (!unitMap.TryGetValue(driver, out var fromPos)) continue;
                if (!unitNextMap.TryGetValue(driver, out var toPos)) continue;
                var isAtoB = rangeA.IsIn(fromPos) && rangeB.IsIn(toPos);
                var isBtoA = !isAtoB && rangeB.IsIn(fromPos) && rangeA.IsIn(toPos);
                var flowDensity = 1f;
                if (isAtoB || isBtoA)
                {
                    var passed = GetTilesOnLine(1, fromPos, toPos, 1);
                    if (passed.Count == 0) continue;
                    var toRange = isAtoB? rangeB : rangeA;
                    var passedInTo = passed.Count(toTile => toRange.IsIn(toTile.Index));
                    flowDensity = (float)passedInTo / passed.Count;
                }
                if (isAtoB)
                    fromAtoB += flowDensity;
                else if (isBtoA)
                    fromBtoA += flowDensity;
            }
            return (fromAtoB, fromBtoA);
        }

        public static (int fromAtoB, int fromBtoA) GetFlow(PositionRange rangeA, PositionRange rangeB, 
            int unitFrom, int unitTo)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据未完成预处理");
                _task?.Wait();
            }
            
            var service = ServiceCommon.Instance;
            int fromAtoB = 0;
            int fromBtoA = 0;
            
            var currMap = _positions.GetValueOrDefault(unitFrom, _emptyMap);
            Dictionary<Driver, Position> nextMap = [];
            /**
             * 记录每个司机的状态
             * 0: 未开始
             * 1: 从A出发（已进入过A）
             * 2: 从B出发（已进入过B）
             */
            Dictionary<Driver, int> states = [];
            for (var unit = unitFrom; unit <= unitTo; unit++, currMap = nextMap )
            {
                nextMap = _positions.GetValueOrDefault(unit, _emptyMap);
                if (currMap.Count == 0 || nextMap.Count == 0) continue;
                
                foreach (var entry in currMap)
                {
                    var driver = entry.Key;
                    if (!nextMap.TryGetValue(driver, out var toPos)) continue;
                    var fromPos = entry.Value;
                    if (fromPos.GetTile() == toPos.GetTile()) continue;
                    
                    // 如果广义范围都和A、B范围不相交，就跳过
                    var maybeRange = PositionRange.FromUnsort(fromPos, toPos);
                    if (!maybeRange.IsIntersect(rangeA) && !maybeRange.IsIntersect(rangeB)) continue;
                    
                    // 使用新的几何计算方法判断线段是否经过两个范围及顺序
                    var result = service.CheckLinePassThroughTwoRanges(fromPos, toPos, rangeA, rangeB);
                    
                    var state = states.GetValueOrDefault(driver, 0);
                    
                    // 根据返回值处理不同情况
                    switch (result)
                    {
                        case 3: // 先经过range1(A)，再经过range2(B)
                            if (state is 0 or 1)
                            {
                                fromAtoB++;
                                state = 2; // 标记已完成A->B
                            }
                            break;
                        case 4: // 先经过range2(B)，再经过range1(A)
                            if (state is 0 or 2)
                            {
                                fromBtoA++;
                                state = 1; // 标记已完成B->A
                            }
                            break;
                        case 1: // 只经过rangeA
                            if (state == 0)
                                state = 1;
                            break;
                        case 2: // 只经过rangeB
                            if (state == 0)
                                state = 2;
                            break;
                        // case 0: 不经过任何范围，不做处理
                    }
                    
                    if (state != 0)
                        states[driver] = state;
                }
            }
            return (fromAtoB, fromBtoA);
        }
        
        
    }
}
