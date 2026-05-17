using GMap.NET;
using System.Diagnostics;
using TaxiManager.Structure;

namespace TaxiManager.Service;

public class ServiceF9 : IServiceF9
{
    public static readonly ServiceF9 Instance = new();

    /// <summary>
    /// 获取从区域A到区域B的最短通行时间路径
    /// 使用速度×流量的双因素权重，优先选择高流量高速度的主干道
    /// </summary>
    public (MapRoute path, double distance, TimeSpan time)? GetShortestPath(PositionRange rangeA, PositionRange rangeB, DateTime time)
    {
        if (!Speeds.Loaded || !Paths.Loaded)
        {
            MessageBox.Show("数据正在加载中，请稍等...");
            return null;
        }

        var sw = Stopwatch.StartNew();
        
        // 使用Speeds的瓦片大小进行分析
        const byte tileSize = Speeds.AnalyzeTileSize;
        
        // 获取指定时间的速率图
        var speedsMap = Speeds.GetSpeeds(time);
        if (speedsMap == null || speedsMap.Count == 0)
        {
            MessageBox.Show($"在{time.Hour}时段没有可用的速率数据");
            return null;
        }

        // 获取总流量图（反映道路的真实通行能力）
        var flowsMap = Paths.FlowsTotal;
        if (flowsMap == null || flowsMap.Count == 0)
        {
            MessageBox.Show("流量数据不可用");
            return null;
        }

        // 获取区域A和B中的所有瓦片
        var tilesInA = Tile.GetTilesIn(tileSize, rangeA);
        var tilesInB = Tile.GetTilesIn(tileSize, rangeB);
        
        if (tilesInA.Count == 0 || tilesInB.Count == 0)
        {
            MessageBox.Show("区域A或B不包含有效瓦片");
            return null;
        }

        // 筛选出同时有速率和流量数据的瓦片
        var startTiles = tilesInA.Where(t => 
            speedsMap.ContainsKey(t) && speedsMap[t] > 0 && 
            flowsMap.ContainsKey(t) && flowsMap[t] > 0).ToList();
        var endTiles = tilesInB.Where(t => 
            speedsMap.ContainsKey(t) && speedsMap[t] > 0 && 
            flowsMap.ContainsKey(t) && flowsMap[t] > 0).ToList();
        
        if (startTiles.Count == 0 || endTiles.Count == 0)
        {
            MessageBox.Show("区域A或B中没有有效的速度和流量数据，请尝试调整区域位置");
            return null;
        }

        // 使用改进的Dijkstra算法，基于速度×流量权重寻找最优路径
        var result = FindOptimalPath(startTiles, endTiles, speedsMap, flowsMap, tileSize);
        
        sw.Stop();
        
        if (result == null)
        {
            return null;
        }

        var (pathTiles, totalDistance, totalTime) = result.Value;
        
        // 将瓦片路径转换为地图路径点
        var points = new List<PointLatLng>();
        foreach (var tile in pathTiles)
        {
            var center = GetTileCenter(tile);
            points.Add(center.ToGmap());
        }

        // 创建地图路线
        var route = new MapRoute(points, "F9_CommunicationPath");

        return (route, totalDistance, TimeSpan.FromHours(totalTime));
    }

    /// <summary>
    /// 使用改进的Dijkstra算法寻找最优路径
    /// 权重 = 距离 / (速度 × 流量)，优先选择高流量高速度的主干道
    /// </summary>
    private (List<Tile> path, double distance, double totalTime)? FindOptimalPath(
        List<Tile> startTiles, 
        List<Tile> endTiles, 
        Dictionary<Tile, float> speedsMap,
        Dictionary<Tile, float> flowsMap,
        byte tileSize)
    {
        // 构建终点集合以便快速查找
        var endTileSet = new HashSet<Tile>(endTiles);
        
        // Dijkstra算法数据结构
        var dist = new Dictionary<Tile, double>(); // 从起点到各瓦片的最小代价
        var prev = new Dictionary<Tile, Tile?>();   // 前驱节点
        var visited = new HashSet<Tile>();
        
        // 初始化所有起点
        foreach (var startTile in startTiles)
        {
            dist[startTile] = 0;
            prev[startTile] = null;
        }
        
        // 优先队列（使用SortedList模拟）
        var priorityQueue = new SortedList<double, List<Tile>>();
        foreach (var startTile in startTiles)
        {
            if (!priorityQueue.ContainsKey(0))
                priorityQueue[0] = new List<Tile>();
            priorityQueue[0].Add(startTile);
        }
        
        Tile? foundEndTile = null;
        
        while (priorityQueue.Count > 0)
        {
            // 取出代价最小的节点
            var currentDist = priorityQueue.Keys[0];
            var currentTiles = priorityQueue.Values[0];
            priorityQueue.RemoveAt(0);
            
            foreach (var currentTile in currentTiles)
            {
                if (visited.Contains(currentTile))
                    continue;
                
                visited.Add(currentTile);
                
                // 检查是否到达终点区域
                if (endTileSet.Contains(currentTile))
                {
                    foundEndTile = currentTile;
                    break;
                }
                
                // 获取相邻瓦片
                var neighbors = GetAdjacentTilesWithWeight(currentTile, speedsMap, flowsMap);
                
                foreach (var (neighbor, weight) in neighbors)
                {
                    if (visited.Contains(neighbor))
                        continue;
                    
                    // 新代价 = 当前代价 + 边的权重
                    var newDist = currentDist + weight;
                    
                    // 如果找到更优的路径，更新
                    if (!dist.ContainsKey(neighbor) || newDist < dist[neighbor])
                    {
                        dist[neighbor] = newDist;
                        prev[neighbor] = currentTile;
                        
                        // 加入优先队列
                        if (!priorityQueue.ContainsKey(newDist))
                            priorityQueue[newDist] = new List<Tile>();
                        priorityQueue[newDist].Add(neighbor);
                    }
                }
            }
            
            if (foundEndTile != null)
                break;
        }
        
        if (foundEndTile == null)
        {
            return null; // 未找到路径
        }
        
        // 重建路径
        var path = ReconstructPath(prev, foundEndTile.Value);
        
        // 计算实际通行时间（小时）
        var (totalDistance, totalTime) = CalculateTravel(path, speedsMap, tileSize);
        
        return (path, totalDistance, totalTime);
    }

    /// <summary>
    /// 获取相邻瓦片及其权重
    /// 权重 = 距离 / (速度 × 流量系数)
    /// 流量系数用于放大高流量道路的优先级
    /// </summary>
    private static List<(Tile tile, double weight)> GetAdjacentTilesWithWeight(
        Tile tile, 
        Dictionary<Tile, float> speedsMap,
        Dictionary<Tile, float> flowsMap)
    {
        var adjacent = new List<(Tile, double)>();
        
        // 4邻域：上、下、左、右
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };
        
        var distance = tile.Size * 100.0; // 瓦片边长（米）
        
        for (int i = 0; i < 4; i++)
        {
            var neighborX = (int)tile.X + dx[i];
            var neighborY = (int)tile.Y + dy[i];
            
            if (neighborX < 0 || neighborY < 0)
                continue;
            
            var neighbor = Tile.From(tile.Size, (uint)neighborX, (uint)neighborY);
            
            // 必须同时有速度和流量数据
            if (!speedsMap.TryGetValue(neighbor, out var speed) || speed <= 0)
                continue;
            if (!flowsMap.TryGetValue(neighbor, out var flow) || flow <= 0)
                continue;
            
            // 计算权重：距离 / (速度 × 流量系数)
            // 流量系数使用log缩放，避免流量差异过大
            var flowFactor = Math.Log(1 + flow); // log缩放
            var weight = distance / (speed * flowFactor);
            
            adjacent.Add((neighbor, weight));
        }
        
        // 按权重排序，优先探索低权重（高速度高流量）的瓦片
        return adjacent.OrderBy(x => x.Item2).ToList();
    }

    /// <summary>
    /// 计算路径的实际通行需要
    /// </summary>
    private static (double totalDistance, double totalTime) CalculateTravel(List<Tile> path, Dictionary<Tile, float> speedsMap, byte tileSize)
    {
        if (path.Count < 2) return (0, 0);
        
        double totalDistance = 0;
        double totalTime = 0;
        
        var distance = tileSize * 100.0 / 1000.0; // 转换为公里
        
        for (int i = 1; i < path.Count; i++)
        {
            var speed = speedsMap.GetValueOrDefault(path[i], 0);
            if (speed > 0)
            {
                totalDistance += distance;
                totalTime += (distance / speed); // 时间（小时）
                //Console.WriteLine($"{distance}:{speed}");
            }
        }
        return (totalDistance, totalTime);
    }

    /// <summary>
    /// 根据前驱节点重建路径
    /// </summary>
    private static List<Tile> ReconstructPath(Dictionary<Tile, Tile?> prev, Tile endTile)
    {
        var path = new List<Tile>();
        var tile = endTile;
        
        while (tile != null)
        {
            path.Add(tile);
            
            if (!prev.ContainsKey(tile) || prev[tile] == null)
                break;
            
            tile = prev[tile].Value;
        }
        
        path.Reverse();
        return path;
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
}
