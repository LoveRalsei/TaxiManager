using System.Diagnostics;
using System.Linq;

namespace TaxiManager.Structure;

public class Paths
{
    public const byte AnalyzeTileSize = 3;
    private static Task? _task;
    public static bool Loaded => _task?.IsCompleted ?? false;
    
    private static Dictionary<Tile, float>? _flowsTotal;
    public static Dictionary<Tile, float> FlowsTotal
    {
        get
        {
            if (Loaded) return _flowsTotal!;
            MessageBox.Show("尚未完成路径数据预处理");
            _task?.Wait();
            return _flowsTotal!;
        }
    }

    private static List<FrequentPath>? _frequentPaths;
    public static List<FrequentPath> FrequentPaths
    {
        get
        {
            if (Loaded) return _frequentPaths!;
            MessageBox.Show("尚未完成路径数据预处理");
            _task?.Wait();
            return _frequentPaths!;
        }
    }

    public static void Initialize()
    {
        _task = Density.ExecuteAfterLoaded(() =>
        {
            Console.WriteLine("Initializing Paths");
            var sw = Stopwatch.StartNew();
            int startUnit = TimeUnit.GetUnit(DataLoader.TimeMin);
            int endUnit = TimeUnit.GetUnit(DataLoader.TimeMax);

            Dictionary<Tile, float> flowsTotal = [];
            
            Dictionary<Tile, float> currDensityMap = Density.GetDensity(AnalyzeTileSize, startUnit);
            Dictionary<Tile, float> nextDensityMap = [];
            for (var unit = startUnit; unit < endUnit; unit++, currDensityMap = nextDensityMap)
            {
                nextDensityMap = Density.GetDensity(AnalyzeTileSize, unit + 1);
                foreach (var (tile, density) in currDensityMap)
                {
                    if (!nextDensityMap.TryGetValue(tile, out var densityNext)) continue;
                    var delta = Math.Abs(density - densityNext);
                    flowsTotal[tile] = flowsTotal.GetValueOrDefault(tile) + delta;
                }
            }
            _flowsTotal = flowsTotal;
            
            // 基于流量图提取频繁路径
            _frequentPaths = ExtractFrequentPaths(flowsTotal);
            
            sw.Stop();
            Console.WriteLine($"Extracted {_frequentPaths.Count} frequent paths");
            Console.WriteLine($"Paths Initialized in {sw.ElapsedMilliseconds}ms");
        });
    }

    /// <summary>
    /// 从流量图中提取频繁路径
    /// </summary>
    public static List<FrequentPath> ExtractFrequentPaths(Dictionary<Tile, float> flows)
    {
        var frequentPaths = new List<FrequentPath>();
        
        // 设置流量阈值，只考虑高流量瓦片
        var flowValues = flows.Values.OrderByDescending(v => v).ToList();
        var thresholdIndex = Math.Min(flowValues.Count / 2, flowValues.Count); // 取前50%作为高流量
        var flowThreshold = thresholdIndex > 0 ? flowValues[thresholdIndex - 1] : 0;
        
        Console.WriteLine($"[Paths] Flow threshold: {flowThreshold:F4}, Total tiles: {flows.Count}, High flow tiles: {flowValues.Count(t => t >= flowThreshold)}");
        
        // 获取高流量瓦片
        var highFlowTiles = flows
            .Where(kv => kv.Value >= flowThreshold && kv.Value > 0)
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToList();
        
        // 使用贪心算法构建路径
        var visited = new HashSet<Tile>();
        
        foreach (var startTile in highFlowTiles)
        {
            if (visited.Contains(startTile)) continue;
            
            // 从当前瓦片开始构建路径
            var path = BuildPathFromTile(startTile, highFlowTiles, visited, flows);
            
            if (path.PathTiles.Count >= 3) // 至少包含3个瓦片才认为是有效路径
            {
                frequentPaths.Add(path);
            }
        }
        
        // 合并相似路径
        var mergedPaths = MergeSimilarPaths(frequentPaths);
        
        Console.WriteLine($"[Paths] Extracted {frequentPaths.Count} paths, merged to {mergedPaths.Count} paths");
        
        // 按频率排序
        var sortedPaths = mergedPaths.OrderByDescending(p => p.Frequency).ToList();
        
        return sortedPaths;
    }

    /// <summary>
    /// 提取从区域A到区域B的频繁路径
    /// </summary>
    public static List<FrequentPath> ExtractFrequentPaths(
        Dictionary<Tile, float> flows,
        PositionRange regionA,
        PositionRange regionB)
    {
        if (flows.Count == 0)
            return [];
        if (flows.Keys.First().Size != AnalyzeTileSize)
            throw new ArgumentException($"Invalid tile size {flows.Keys.First().Size}, it should be {AnalyzeTileSize}.");
        
        var tilesInA = Tile.GetTilesIn(AnalyzeTileSize, regionA);
        var tilesInB = Tile.GetTilesIn(AnalyzeTileSize, regionB);
        if (tilesInA.Count == 0 || tilesInB.Count == 0)
            return [];
        
        // 获取区域A和B中所有有流量的瓦片
        var startTiles = tilesInA.Where(t => flows.ContainsKey(t) && flows[t] > 0).ToList();
        var endTiles = tilesInB.Where(t => flows.ContainsKey(t) && flows[t] > 0).ToList();
        
        // 使用BFS/DFS寻找所有可能的路径，然后按流量排序
        var candidatePaths = new List<FrequentPath>();
        
        // 限制起点和终点数量以提高性能
        int maxStartPoints = (int)Math.Min(Math.Sqrt(startTiles.Count), 10);
        int maxEndPoints = (int)Math.Min(Math.Sqrt(endTiles.Count), 10);
        
        var selectedStarts = startTiles.OrderByDescending(t => flows[t]).Take(maxStartPoints).ToList();
        var selectedEnds = endTiles.OrderByDescending(t => flows[t]).Take(maxEndPoints).ToList();
        foreach (var startTile in selectedStarts)
        {
            foreach (var endTile in selectedEnds)
            {
                if (startTile.Equals(endTile)) continue;
                
                // 使用BFS寻找最短路径（基于流量权重）
                var path = FindPathByBFS(startTile, endTile, flows);
                if (path != null && path.PathTiles.Count >= 2)
                    candidatePaths.Add(path);
            }
        }
        // 去重 - 复用Paths中的公共方法
        var uniquePaths = Paths.MergeSimilarPaths(candidatePaths);
        // 按频率排序并返回前K个
        return uniquePaths.OrderByDescending(p => p.Frequency).ToList();
    }
    
    /// <summary>
    /// 从指定瓦片开始构建路径，用于无目标自动分析路径
    /// </summary>
    private static FrequentPath BuildPathFromTile(Tile startTile, List<Tile> highFlowTiles, 
        HashSet<Tile> visited, Dictionary<Tile, float> flows)
    {
        var pathTiles = new List<Tile>();
        Tile? currentTile = startTile;
        
        while (currentTile != null && !visited.Contains(currentTile.Value))
        {
            pathTiles.Add(currentTile.Value);
            visited.Add(currentTile.Value);
            
            // 寻找下一个最可能的相邻瓦片
            //var neighbors = GetAdjacentTilesWithFlow(currentTile.Value, flows, visited).Where(t => highFlowTilesSet.Contains(t));
            Tile? bestNextTile = null;
            float maxFlow = 0;
            // 查找与当前瓦片相邻且未访问的高流量瓦片
            foreach (var tile in highFlowTiles)
            {
                if (visited.Contains(tile)) continue;
            
                // 检查是否相邻
                if (!IsAdjacent(currentTile.Value, tile)) continue;
                var flow = flows.GetValueOrDefault(tile, 0);
                if (flow <= maxFlow) continue;
                maxFlow = flow;
                bestNextTile = tile;
            }

            currentTile = bestNextTile;
            //currentTile = neighbors.First();
        }
        
        // 计算路径的总频率和长度
        var frequency = pathTiles.Sum(tile => flows.GetValueOrDefault(tile, 0));
        var lengthMeters = CalculatePathLength(pathTiles);
        
        return new FrequentPath
        {
            PathTiles = pathTiles,
            Frequency = frequency,
            LengthMeters = lengthMeters
        };
    }

    /// <summary>
    /// 使用BFS寻找从起点到终点的路径
    /// </summary>
    private static FrequentPath? FindPathByBFS(Tile startTile, Tile endTile, 
        Dictionary<Tile, float> flows)
    {
        // BFS队列
        var queue = new Queue<Tile>();
        queue.Enqueue(startTile);
        
        // 记录访问过的节点和前驱节点
        var visited = new HashSet<Tile> { startTile };

        var cameFrom = new Dictionary<Tile, Tile>
        {
            [startTile] = startTile // 起点的前驱是自己
        };

        var found = false;
        
        while (queue.Count > 0 && !found)
        {
            var currentTile = queue.Dequeue();
            
            // 获取所有相邻且有流量的瓦片
            var neighbors = GetAdjacentTilesWithFlow(currentTile, flows, visited);
            
            foreach (var neighbor in neighbors)
            {
                visited.Add(neighbor);
                cameFrom[neighbor] = currentTile;
                
                // 如果到达终点，立即返回
                if (neighbor.Equals(endTile))
                {
                    found = true;
                    break;
                }

                queue.Enqueue(neighbor);
            }
        }
        
        if (!found || !cameFrom.ContainsKey(endTile))
        {
            return null;
        }
        
        // 重建路径
        return ReconstructPath(cameFrom, endTile, flows);
    }
    
    /// <summary>
    /// 获取相邻的有流量的瓦片，并且有序
    /// </summary>
    private static List<Tile> GetAdjacentTilesWithFlow(Tile tile, Dictionary<Tile, float> flows, HashSet<Tile>? visited = null)
    {
        List<Tile> adjacent = [];
        
        // 4邻域：上、下、左、右（不包含对角线）
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };
        
        for (var i = 0; i < 4; i++)
        {
            var neighborX = (int)tile.X + dx[i];
            var neighborY = (int)tile.Y + dy[i];
            
            if (neighborX < 0 || neighborY < 0) continue;
            
            var neighbor = Tile.From(tile.Size, (uint)neighborX, (uint)neighborY);
            
            if (visited?.Contains(neighbor) ?? false) continue;
            
            // 只要该瓦片在流量图中且流量>0即可
            if (flows.ContainsKey(neighbor) && flows[neighbor] > 0)
            {
                adjacent.Add(neighbor);
            }
        }
        
        return adjacent.OrderByDescending(n => flows[n]).ToList();;
    }
    
    /// <summary>
    /// 根据前驱节点重建路径，因为BFS搜索是无序的，所以需要根据前驱节点进行排序
    /// </summary>
    private static FrequentPath ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current, 
        Dictionary<Tile, float> flows)
    {
        var pathTiles = new List<Tile>();
        var tile = current;
        
        // 沿着前驱节点回溯到起点
        while (true)
        {
            pathTiles.Add(tile);
            
            // 如果到达起点（起点的前驱是自己），停止
            if (cameFrom[tile].Equals(tile))
            {
                break;
            }
            
            tile = cameFrom[tile];
        }
        
        // 反转得到从起点到终点的路径
        pathTiles.Reverse();
        
        var frequency = pathTiles.Sum(t => flows.GetValueOrDefault(t, 0));
        // 复用Paths中的公共方法计算路径长度
        var lengthMeters = Paths.CalculatePathLength(pathTiles);
        
        return new FrequentPath
        {
            PathTiles = pathTiles,
            Frequency = frequency,
            LengthMeters = lengthMeters
        };
    }
    
    /// <summary>
    /// 判断两个瓦片是否相邻
    /// </summary>
    private static bool IsAdjacent(Tile tile1, Tile tile2)
        => tile1.Size == tile2.Size && 
           Math.Abs((int)tile1.X - (int)tile2.X) + Math.Abs((int)tile1.Y - (int)tile2.Y) == 1;

    /// <summary>
    /// 计算路径长度（米）
    /// </summary>
    private static double CalculatePathLength(List<Tile> pathTiles)
    {
        if (pathTiles.Count < 2) return 0;
        
        double totalLength = 0;
        for (int i = 0; i < pathTiles.Count - 1; i++)
        {
            var tile1 = pathTiles[i];
            var tile2 = pathTiles[i + 1];
            
            // 使用瓦片的实际大小来计算距离
            // 相邻瓦片中心点之间的距离等于瓦片大小（以米为单位）
            var tileSize = tile1.SizeMeter; // 瓦片大小（米）
            
            // 计算两个瓦片中心的偏移量
            var dx = Math.Abs((int)tile1.X - (int)tile2.X);
            var dy = Math.Abs((int)tile1.Y - (int)tile2.Y);
            
            // 实际距离 = sqrt(dx^2 + dy^2) * 瓦片大小
            // 对于相邻瓦片，dx和dy通常是0或1
            var distance = Math.Sqrt(dx * dx + dy * dy) * tileSize;
            
            totalLength += distance;
        }
        
        return totalLength;
    }

    /// <summary>
    /// 合并相似路径
    /// </summary>
    private static List<FrequentPath> MergeSimilarPaths(List<FrequentPath> paths)
    {
        if (paths.Count <= 1) return paths;
        
        var mergedPaths = new List<FrequentPath>();
        var used = new bool[paths.Count];
        
        for (int i = 0; i < paths.Count; i++)
        {
            if (used[i]) continue;
            
            var basePath = paths[i];
            var similarPaths = new List<FrequentPath> { basePath };
            used[i] = true;
            
            // 寻找与当前路径相似的其他路径
            for (int j = i + 1; j < paths.Count; j++)
            {
                if (used[j]) continue;
                
                if (ArePathsSimilar(basePath, paths[j], 0.6))
                {
                    similarPaths.Add(paths[j]);
                    used[j] = true;
                }
            }
            
            // 合并相似路径
            var mergedPath = similarPaths.OrderByDescending(p => p.Frequency).First();;
            mergedPaths.Add(mergedPath);
        }
        
        return mergedPaths;
    }

    /// <summary>
    /// 判断两条路径是否相似
    /// </summary>
    private static bool ArePathsSimilar(FrequentPath path1, FrequentPath path2, double threshold = 0.7)
    {
        // 如果路径长度差异太大，不相似
        if (Math.Abs(path1.LengthMeters - path2.LengthMeters) > 
            Math.Max(path1.LengthMeters, path2.LengthMeters) * 0.5)
            return false;
        
        // 计算路径间的重叠度
        var tiles1 = new HashSet<Tile>(path1.PathTiles);
        var tiles2 = new HashSet<Tile>(path2.PathTiles);
        
        var intersection = tiles1.Intersect(tiles2).Count();
        var union = tiles1.Union(tiles2).Count();
        
        // Jaccard相似度
        var similarity = union > 0 ? (double)intersection / union : 0;
        
        return similarity > threshold; // 默认相似度超过60%认为相似
    }
}