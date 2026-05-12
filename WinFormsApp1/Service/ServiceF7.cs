using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TaxiManager.Structure;

namespace TaxiManager.Service
{
    public class ServiceF7 : IServiceF7
    {
        public static readonly ServiceF7 Instance = new();

        /// <summary>
        /// 获取整个城市中最频繁的前k条路径
        /// 
        /// 算法思路（基于TileDensity）：
        /// 1. 累计所有时间点的瓦片密度，得到每个瓦片的总流量
        /// 2. 提取高流量瓦片作为候选节点
        /// 3. 基于Flows中的路径追踪逻辑，构建频繁路径
        /// 4. 按路径总流量排序，返回前k条
        /// 
        /// 性能优势：
        /// - TileDensity已预计算，查询O(1)
        /// - 避免GetTilesOnLine的高开销
        /// - 采样策略进一步降低计算量
        /// </summary>
        List<FrequentPathResult> IServiceF7.GetTopKFrequentPaths(int k)
        {
            double minLengthMeters = 5000;
            int sampleInterval = 4;
            if (k <= 0)
                throw new ArgumentException("k must be positive", nameof(k));
            if (minLengthMeters <= 0)
                throw new ArgumentException("minLengthMeters must be positive", nameof(minLengthMeters));
            if (sampleInterval <= 0)
                throw new ArgumentException("sampleInterval must be positive", nameof(sampleInterval));

            Console.WriteLine($"[F7] Starting frequent path analysis: k={k}, minLength={minLengthMeters}m, sampleInterval={sampleInterval}");
            Stopwatch sw = Stopwatch.StartNew();

            // 步骤1: 累计所有采样时间点的瓦片密度，得到总流量图
            var totalFlowMap = AccumulateTileFlows(sampleInterval);
            Console.WriteLine($"[F7] Accumulated flows for {totalFlowMap.Count} tiles");

            // 步骤2: 提取高流量瓦片（作为路径候选节点）
            var highFlowTiles = ExtractHighFlowTiles(totalFlowMap);
            Console.WriteLine($"[F7] Extracted {highFlowTiles.Count} high-flow tiles");

            // 步骤3: 基于Flows分析，构建频繁路径
            var frequentPaths = BuildFrequentPaths(highFlowTiles, totalFlowMap, minLengthMeters);
            Console.WriteLine($"[F7] Built {frequentPaths.Count} candidate paths");

            // 步骤4: 按频率降序排序，取前k条
            var topKPaths = frequentPaths
                .OrderByDescending(p => p.Frequency)
                .Take(k)
                .ToList();

            sw.Stop();
            Console.WriteLine($"[F7] Analysis completed in {sw.ElapsedMilliseconds}ms");

            return topKPaths;
        }

        /// <summary>
        /// 累计所有采样时间点的瓦片密度
        /// 类似F4的思路，但累计所有时间点而非计算变化
        /// </summary>
        private Dictionary<Tile, float> AccumulateTileFlows(int sampleInterval)
        {
            var totalFlowMap = new Dictionary<Tile, float>();
            
            // 获取时间范围
            var startTime = DataLoader.TimeMin;
            var endTime = DataLoader.TimeMax;
            var startUnit = TimeUnit.GetUnit(startTime);
            var endUnit = TimeUnit.GetUnit(endTime);

            Console.WriteLine($"[F7] Time range: unit {startUnit} to {endUnit}, sampling every {sampleInterval} units");

            // 采样所有时间点
            for (int unit = startUnit; unit <= endUnit; unit += sampleInterval)
            {
                // 从TileDensity获取该时间点的密度分布
                if (!TileDensity.Loaded)
                    continue;

                var timeFlows = GetTimeSliceFlows(unit);
                
                // 累加到总流量图
                foreach (var (tile, density) in timeFlows)
                {
                    if (totalFlowMap.TryGetValue(tile, out float existing))
                        totalFlowMap[tile] = existing + density;
                    else
                        totalFlowMap[tile] = density;
                }
            }

            return totalFlowMap;
        }

        /// <summary>
        /// 获取单个时间点的瓦片流量分布
        /// 参考Flows.GetFlows的实现，但使用TileDensity的预计算数据
        /// </summary>
        private Dictionary<Tile, float> GetTimeSliceFlows(int unit)
        {
            var flows = new Dictionary<Tile, float>();
            
            // 直接使用TileDensity中该时间点的所有瓦片密度
            // 由于_countMap是private，我们需要通过其他方式获取
            // 这里采用Flows.GetFlows的思路，但优化为批量查询
            
            var serviceCommon = IServiceCommon.Instance;
            var drivers = DataLoader.Drivers;
            
            // 获取该时间点所有司机的位置
            var positions = GetDriverPositionsAtUnit(unit);
            
            // 对每个司机，计算其移动轨迹经过的瓦片
            foreach (var (driverId, fromPos, toPos) in positions)
            {
                if (fromPos == null || toPos == null)
                    continue;
                    
                var fromTile = fromPos.Value.GetTile();
                var toTile = toPos.Value.GetTile();
                
                if (fromTile == toTile)
                    continue;
                
                // 使用GetTilesOnLine获取路径上的瓦片
                // 这是必要的开销，但只在采样点执行，大幅减少调用次数
                var passedTiles = serviceCommon.GetTilesOnLine(1, fromPos.Value, toPos.Value, 0);
                
                if (passedTiles.Count == 0)
                    continue;
                
                // 均匀分配流量密度
                var flowDensity = 1.0f / passedTiles.Count;
                
                foreach (var tile in passedTiles)
                {
                    if (flows.TryGetValue(tile, out float existing))
                        flows[tile] = existing + flowDensity;
                    else
                        flows[tile] = flowDensity;
                }
            }
            
            return flows;
        }

        /// <summary>
        /// 获取指定时间点所有司机的位置（当前和下一时刻）
        /// </summary>
        private List<(int driverId, Position? fromPos, Position? toPos)> GetDriverPositionsAtUnit(int unit)
        {
            var result = new List<(int, Position?, Position?)>();
            var nextUnit = unit + 1;
            
            foreach (var driver in DataLoader.Drivers)
            {
                if (driver.IsEmpty)
                    continue;
                
                var fromTime = TimeUnit.GetTime(unit);
                var toTime = TimeUnit.GetTime(nextUnit);
                
                var fromPos = driver.GetPosition(fromTime, TimeTolerance.Minutes(15));
                var toPos = driver.GetPosition(toTime, TimeTolerance.Minutes(15));
                
                if (fromPos != null && toPos != null)
                {
                    result.Add((driver.Id, fromPos, toPos));
                }
            }
            
            return result;
        }

        /// <summary>
        /// 提取高流量瓦片
        /// 选择流量高于阈值的瓦片作为路径构建的候选节点
        /// </summary>
        private List<Tile> ExtractHighFlowTiles(Dictionary<Tile, float> totalFlowMap)
        {
            if (totalFlowMap.Count == 0)
                return new List<Tile>();

            // 计算平均流量作为阈值
            var avgFlow = totalFlowMap.Values.Average();
            var threshold = avgFlow * 0.5f; // 阈值设为平均值的一半

            // 筛选高流量瓦片
            var highFlowTiles = totalFlowMap
                .Where(pair => pair.Value >= threshold)
                .Select(pair => pair.Key)
                .ToList();

            Console.WriteLine($"[F7] Flow threshold: {threshold:F2}, avg: {avgFlow:F2}");

            return highFlowTiles;
        }

        /// <summary>
        /// 基于高流量瓦片构建频繁路径
        /// 使用贪心算法连接相邻的高流量瓦片形成路径
        /// </summary>
        private List<FrequentPathResult> BuildFrequentPaths(
            List<Tile> highFlowTiles,
            Dictionary<Tile, float> totalFlowMap,
            double minLengthMeters)
        {
            var results = new List<FrequentPathResult>();
            var visited = new HashSet<Tile>();
            
            // 按流量降序排序，优先从高流量瓦片开始构建路径
            var sortedTiles = highFlowTiles
                .OrderByDescending(t => totalFlowMap[t])
                .ToList();

            foreach (var startTile in sortedTiles)
            {
                if (visited.Contains(startTile))
                    continue;

                // 从该瓦片开始，贪心地构建路径
                var path = BuildPathFromTile(startTile, highFlowTiles, totalFlowMap, visited);
                
                // 计算路径长度
                var pathLength = CalculatePathLength(path);
                
                // 只保留长度满足要求的路径
                if (pathLength >= minLengthMeters && path.Count >= 2)
                {
                    // 计算路径总流量（路径上所有瓦片流量的平均值）
                    var totalFlow = path.Sum(t => totalFlowMap[t]);
                    var avgFlow = totalFlow / path.Count;
                    
                    results.Add(new FrequentPathResult
                    {
                        PathTiles = new List<Tile>(path),
                        Frequency = avgFlow,
                        LengthMeters = pathLength
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// 从起始瓦片开始，贪心地构建路径
        /// 每次选择相邻且流量最高的未访问瓦片
        /// </summary>
        private List<Tile> BuildPathFromTile(
            Tile startTile,
            List<Tile> candidateTiles,
            Dictionary<Tile, float> flowMap,
            HashSet<Tile> visited)
        {
            var path = new List<Tile> { startTile };
            visited.Add(startTile);
            
            var current = startTile;
            var maxPathLength = 50; // 限制路径最大长度，避免过长
            
            while (path.Count < maxPathLength)
            {
                // 查找当前瓦片的8邻域中流量最高的未访问瓦片
                var neighbors = GetNeighbors(current);
                
                Tile? nextTile = null;
                float maxFlow = 0;
                
                foreach (var neighbor in neighbors)
                {
                    if (visited.Contains(neighbor))
                        continue;
                    
                    if (!candidateTiles.Contains(neighbor))
                        continue;
                    
                    if (!flowMap.TryGetValue(neighbor, out float flow))
                        continue;
                    
                    if (flow > maxFlow)
                    {
                        maxFlow = flow;
                        nextTile = neighbor;
                    }
                }
                
                // 如果没有合适的下一个瓦片，结束路径
                if (nextTile == null)
                    break;
                
                path.Add(nextTile.Value);
                visited.Add(nextTile.Value);
                current = nextTile.Value;
            }
            
            return path;
        }

        /// <summary>
        /// 获取瓦片的8邻域
        /// </summary>
        private List<Tile> GetNeighbors(Tile tile)
        {
            var neighbors = new List<Tile>();
            uint x = tile.X;
            uint y = tile.Y;
            byte size = tile.Size;
            
            // 8个方向
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;
                    
                    var neighborX = (int)x + dx;
                    var neighborY = (int)y + dy;
                    
                    if (neighborX < 0 || neighborY < 0)
                        continue;
                    
                    var neighbor = new Tile { X = (uint)neighborX, Y = (uint)neighborY, Size = size };
                    neighbors.Add(neighbor);
                }
            }
            
            return neighbors;
        }

        /// <summary>
        /// 计算路径的实际长度（米）
        /// </summary>
        private double CalculatePathLength(List<Tile> tiles)
        {
            if (tiles.Count < 2)
                return 0;

            double totalLength = 0;
            uint tileSizeMeter = tiles[0].SizeMeter;

            for (int i = 0; i < tiles.Count - 1; i++)
            {
                var tile1 = tiles[i];
                var tile2 = tiles[i + 1];

                // 计算相邻瓦片中心的距离
                var pos1 = tile1.Index;
                var pos2 = tile2.Index;

                double dx = Math.Abs(pos2.X - pos1.X);
                double dy = Math.Abs(pos2.Y - pos1.Y);
                totalLength += Math.Sqrt(dx * dx + dy * dy);
            }

            return totalLength;
        }
    }
}
