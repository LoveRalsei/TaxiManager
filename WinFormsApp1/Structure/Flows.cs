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

            var service = IServiceCommon.Instance;

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
                var passed = service.GetTilesOnLine(tileSize, fromPos, toPos, 1);
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
            
            var service = IServiceCommon.Instance;
            
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
                var passed = service.GetTilesOnLine(tileSize, fromPos, toPos, 2);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rangeA"></param>
        /// <param name="rangeB"></param>
        /// <param name="timeUnit"></param>
        /// <returns></returns>
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
                    var passed = service.GetTilesOnLine(1, fromPos, toPos, 1);
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

        /// <summary>
        /// 获取时段内，两个区域往返的流量
        /// </summary>
        /// <param name="rangeA"></param>
        /// <param name="rangeB"></param>
        /// <param name="unitFrom"></param>
        /// <param name="unitTo"></param>
        /// <returns></returns>
        public static (int fromAtoB, int fromBtoA) GetFlowPeriod(PositionRange rangeA, PositionRange rangeB, 
            int unitFrom, int unitTo)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据未完成预处理");
                _task?.Wait();
            }
            
            var service = IServiceCommon.Instance;
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
                
                foreach (var (driver, fromPos) in currMap)
                {
                    if (!nextMap.TryGetValue(driver, out var toPos)) continue;
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
                            if (state is 1)
                                fromAtoB++;
                            else if (state is 2)
                                fromBtoA++;
                            state = 2;
                            break;
                        case 4: // 先经过range2(B)，再经过range1(A)
                            if (state is 1)
                                fromAtoB++;
                            if (state is  2)
                                fromBtoA++;
                            state = 1;
                            break;
                        case 1: // 只经过rangeA
                            if (state == 2)
                                fromBtoA++;
                            state = 1;
                            break;
                        case 2: // 只经过rangeB
                            if (state == 1)
                                fromAtoB++;
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

        /// <summary>
        /// 这个操作的开销非常高，在满功率状态下也需要1s
        /// </summary>
        /// <param name="tileSize"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static Dictionary<Tile, float> GetFlows(byte tileSize, int unit)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据未完成预处理");
                _task?.Wait();
            }
            
            var flows = new Dictionary<Tile, float>();
            
            var service = IServiceCommon.Instance;
            
            var unitMap = _positions.GetValueOrDefault(unit, _emptyMap);
            var unitNextMap = _positions.GetValueOrDefault(unit + 1, _emptyMap);

            foreach (var (driver, fromPos) in unitMap)
            {
                if (!unitNextMap.TryGetValue(driver, out var toPos)) continue;
                if (fromPos.GetTile(tileSize) == toPos.GetTile(tileSize)) continue;
                
                var passed = service.GetTilesOnLine(tileSize, fromPos, toPos, 0);
                var flowDensity = 1f / passed.Count;
                foreach (var tile in passed)
                    flows[tile] = flows.GetValueOrDefault(tile, 0f) + flowDensity;
            }

            return flows;
        }
        
    }
}
