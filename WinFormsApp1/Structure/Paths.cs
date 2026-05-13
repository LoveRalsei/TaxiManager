using System.Diagnostics;
using System.Linq;

namespace TaxiManager.Structure;

public class Paths
{
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
            
            Dictionary<Tile, float> currDensityMap = Density.GetDensity(3, startUnit);
            Dictionary<Tile, float> nextDensityMap = [];
            for (var unit = startUnit; unit < endUnit; unit++, currDensityMap = nextDensityMap)
            {
                nextDensityMap = Density.GetDensity(3, unit + 1);
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
            Console.WriteLine($"Paths Initialized in {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Extracted {_frequentPaths.Count} frequent paths");
        });
    }

    /// <summary>
    /// 从流量图中提取频繁路径
    /// </summary>
    private static List<FrequentPath> ExtractFrequentPaths(Dictionary<Tile, float> flowsTotal)
    {
        var frequentPaths = new List<FrequentPath>();
        
        // 设置流量阈值，只考虑高流量瓦片
        var flowValues = flowsTotal.Values.OrderByDescending(v => v).ToList();
        var thresholdIndex = Math.Min(flowValues.Count / 2, flowValues.Count); // 取前50%作为高流量
        var flowThreshold = thresholdIndex > 0 ? flowValues[thresholdIndex - 1] : 0;
        
        Console.WriteLine($"[Paths] Flow threshold: {flowThreshold:F4}, Total tiles: {flowsTotal.Count}, High flow tiles: {flowValues.Count(t => t >= flowThreshold)}");
        
        // 获取高流量瓦片
        var highFlowTiles = flowsTotal
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
            var path = BuildPathFromTile(startTile, highFlowTiles, visited, flowsTotal);
            
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
    /// 从指定瓦片开始构建路径
    /// </summary>
    private static FrequentPath BuildPathFromTile(Tile startTile, List<Tile> highFlowTiles, 
        HashSet<Tile> visited, Dictionary<Tile, float> flowsTotal)
    {
        var pathTiles = new List<Tile>();
        Tile? currentTile = startTile;
        
        while (currentTile != null && !visited.Contains(currentTile.Value))
        {
            pathTiles.Add(currentTile.Value);
            visited.Add(currentTile.Value);
            
            // 寻找下一个最可能的相邻瓦片
            var nextTile = FindNextTileInPath(currentTile.Value, highFlowTiles, visited, flowsTotal);
            currentTile = nextTile;
        }
        
        // 计算路径的总频率和长度
        var frequency = pathTiles.Sum(tile => flowsTotal.GetValueOrDefault(tile, 0));
        var lengthMeters = CalculatePathLength(pathTiles);
        
        return new FrequentPath
        {
            PathTiles = pathTiles,
            Frequency = frequency,
            LengthMeters = lengthMeters
        };
    }

    /// <summary>
    /// 在路径中寻找下一个相邻瓦片
    /// </summary>
    private static Tile? FindNextTileInPath(Tile currentTile, List<Tile> highFlowTiles, 
        HashSet<Tile> visited, Dictionary<Tile, float> flowsTotal)
    {
        Tile? bestNextTile = null;
        float maxFlow = 0;
        
        // 查找与当前瓦片相邻且未访问的高流量瓦片
        foreach (var tile in highFlowTiles)
        {
            if (visited.Contains(tile)) continue;
            
            // 检查是否相邻（包括对角线相邻）
            if (IsAdjacent(currentTile, tile))
            {
                var flow = flowsTotal.GetValueOrDefault(tile, 0);
                if (flow > maxFlow)
                {
                    maxFlow = flow;
                    bestNextTile = tile;
                }
            }
        }
        
        return bestNextTile;
    }

    /// <summary>
    /// 判断两个瓦片是否相邻
    /// </summary>
    private static bool IsAdjacent(Tile tile1, Tile tile2)
    {
        // 确保两个瓦片大小相同
        if (tile1.Size != tile2.Size) return false;
        
        // 检查是否在8邻域内相邻
        var dx = Math.Abs((int)tile1.X - (int)tile2.X);
        var dy = Math.Abs((int)tile1.Y - (int)tile2.Y);
        
        return dx <= 1 && dy <= 1 && (dx + dy > 0); // 排除自身
    }

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
    /// 获取瓦片中心点坐标
    /// </summary>
    private static Position GetTileCenter(Tile tile)
    {
        var range = tile.Range;
        var centerX = (range.Min.X + range.Max.X) / 2;
        var centerY = (range.Min.Y + range.Max.Y) / 2;
        return Position.From(centerX, centerY);
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
                
                if (ArePathsSimilar(basePath, paths[j]))
                {
                    similarPaths.Add(paths[j]);
                    used[j] = true;
                }
            }
            
            // 合并相似路径
            var mergedPath = MergePathGroup(similarPaths);
            mergedPaths.Add(mergedPath);
        }
        
        return mergedPaths;
    }

    /// <summary>
    /// 判断两条路径是否相似
    /// </summary>
    private static bool ArePathsSimilar(FrequentPath path1, FrequentPath path2)
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
        
        return similarity > 0.6; // 相似度超过60%认为相似
    }

    /// <summary>
    /// 合并一组相似路径
    /// </summary>
    private static FrequentPath MergePathGroup(List<FrequentPath> similarPaths)
    {
        if (similarPaths.Count == 1) return similarPaths[0];
        
        // 选择频率最高的路径作为基础路径
        var basePath = similarPaths.OrderByDescending(p => p.Frequency).First();
        
        // 合并所有路径的瓦片
        var allTiles = new HashSet<Tile>();
        float totalFrequency = 0;
        
        foreach (var path in similarPaths)
        {
            foreach (var tile in path.PathTiles)
            {
                allTiles.Add(tile);
            }
            totalFrequency += path.Frequency;
        }
        
        // 重新排序瓦片以形成连贯路径
        var orderedTiles = OrderTilesForPath(allTiles.ToList());
        
        // 计算合并后路径的平均长度
        var avgLength = similarPaths.Average(p => p.LengthMeters);
        
        return new FrequentPath
        {
            PathTiles = orderedTiles,
            Frequency = totalFrequency / similarPaths.Count, // 平均频率
            LengthMeters = avgLength
        };
    }

    /// <summary>
    /// 将瓦片列表排序为连贯路径
    /// </summary>
    private static List<Tile> OrderTilesForPath(List<Tile> tiles)
    {
        if (tiles.Count <= 1) return tiles;
        
        var ordered = new List<Tile>();
        var remaining = new List<Tile>(tiles);
        
        // 从第一个瓦片开始
        var current = remaining[0];
        ordered.Add(current);
        remaining.RemoveAt(0);
        
        // 贪心地连接最近的相邻瓦片
        while (remaining.Count > 0)
        {
            Tile? nextTile = null;
            double minDistance = double.MaxValue;
            
            foreach (var tile in remaining)
            {
                if (IsAdjacent(current, tile))
                {
                    // 使用瓦片索引差计算距离
                    var dx = Math.Abs((int)current.X - (int)tile.X);
                    var dy = Math.Abs((int)current.Y - (int)tile.Y);
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nextTile = tile;
                    }
                }
            }
            
            if (nextTile == null)
            {
                // 如果没有相邻瓦片，选择距离最近的瓦片
                nextTile = remaining[0];
                double minDist = double.MaxValue;
                
                foreach (var tile in remaining)
                {
                    var dx = Math.Abs((int)current.X - (int)tile.X);
                    var dy = Math.Abs((int)current.Y - (int)tile.Y);
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    
                    if (distance < minDist)
                    {
                        minDist = distance;
                        nextTile = tile;
                    }
                }
            }
            
            if (nextTile != null)
            {
                ordered.Add(nextTile.Value);
                remaining.Remove(nextTile.Value);
                current = nextTile.Value;
            }
            else
            {
                break;
            }
        }
        
        return ordered;
    }
}