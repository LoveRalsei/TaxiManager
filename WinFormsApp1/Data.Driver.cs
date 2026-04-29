using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    using PathSupplier = Func<List<PathNode>>;
    public class Driver
    {
        /// <summary>
        /// 用于剪枝判断存在范围的瓦片HashSet中，每个瓦片的大小
        /// </summary>
        public const byte ExistTileSize = 1;
        public int Id;
        public readonly List<PathNode> Nodes;
        /// <summary>
        /// 用于剪枝减少对不存在范围中的节点的计算，Tile的Size为ExistTileSize
        /// </summary>
        public readonly HashSet<Tile> TilesExist = [];
        public readonly DateTime MinExist = DateTime.MinValue;
        public readonly DateTime MaxExist = DateTime.MaxValue;
        public bool IsEmpty => Nodes.Count == 0;
        public Driver(List<PathNode> nodes)
        {
            this.Nodes = nodes;
            foreach (var node in Nodes)
            {
                TilesExist.Add(node.Position.GetTile(ExistTileSize));
            }
            TilesExist.TrimExcess();
            if (Nodes.Count > 0)
            {
                MinExist = Nodes.First().Time;
                MaxExist = Nodes.Last().Time;
            }
        }
        public Driver(PathSupplier pathSupplier) : this(pathSupplier()) { }
        
        /// <summary>
        /// 默认时间误差为15分钟。
        /// 获取指定时间点的位置信息，并容许一定的误差，误差范围内的位置信息将被线性插值处理。
        /// 如果不存在误差内的位置信息，则返回null。
        /// </summary>
        public Position? GetPosition(DateTime time, TimeTolerance tolerance = default)
            => GetPositionIndex(time, tolerance)?.position;
        public (uint indexLeft, uint indexRight, Position position)?
            GetPositionIndex(DateTime time, TimeTolerance tolerance = default)
        {
            if (time < MinExist || time > MaxExist) return null;
            if (IsEmpty) return null;

            // 二分查找找到第一个 >= time 的节点
            int left = 0;
            int right = Nodes.Count - 1;
            int index = right;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (Nodes[mid].Time < time)
                {
                    left = mid + 1;
                }
                else
                {
                    index = mid;
                    right = mid - 1;
                }
            }

            // 检查右侧和左侧的节点
            long toleranceMs = tolerance;

            // 右侧节点
            if (index < Nodes.Count)
            {
                var rightNode = Nodes[index];
                long diffRight = (long)(rightNode.Time - time).TotalMilliseconds;
                if (diffRight <= toleranceMs)
                {
                    // 如果正好在目标时间点上
                    if (diffRight == 0) return ((uint)index, (uint)index, rightNode.Position);

                    // 如果左侧有节点，尝试插值
                    if (index > 0)
                    {
                        var leftNode = Nodes[index - 1];
                        long diff = (long)(rightNode.Time - leftNode.Time).TotalMilliseconds;
                        if (diff <= toleranceMs)
                        {
                            long diffLeft = (long)(time - leftNode.Time).TotalMilliseconds;
                            float scale = (float)(diffLeft * 1.0 / diff);
                            return (
                                (uint)index - 1, (uint)index, 
                                Position.Lerp(leftNode.Position, rightNode.Position, scale)!.Value
                                );
                        }
                    }

                    // 无法插值但右侧在容差内
                    return ((uint)index, (uint)index, rightNode.Position);
                }
            }

            // 如果检测右侧节点时未返回值，说明右侧节点无效，只需要再检查左侧节点
            if (index > 0)
            {
                var leftNode = Nodes[index - 1];
                long diffLeft = (long)(time - leftNode.Time).TotalMilliseconds;
                if (diffLeft <= toleranceMs)
                {
                    return ((uint)index - 1, (uint)index - 1, leftNode.Position);
                }
            }

            // 左右节点都无效，返回null
            return null;
        }
        /// <summary>
        /// 获取连续时间段的每一条路线，所谓连续时间段，指的是在可容忍的时间误差内一直在移动
        /// 如果路径中有两个相邻的点，其位置有足够长的时间未更新，视作为不同的连续时间段
        /// </summary>
        public List<MapRoute> GetRoutes(TimeTolerance tolerance = default)
        {
            List<MapRoute> routes = [];
            int index = 0;
            MapRoute curr = new($"Driver#{Id}#{index}");
            DateTime? last = null;
            Nodes.ForEach(node =>
            {
                if (last != null && (node.Time - last.Value).TotalMilliseconds > tolerance.MillisecondsTolerance)
                {
                    routes.Add(curr);
                    index++;
                    curr = new($"Driver#{Id}#{index}");
                }
                last = node.Time;
                curr.Points.Add(node.Position);
            });
            routes.Add(curr);
            return routes;
        }
        public bool IsExist(DateTime from, DateTime to)
        {
            if (from > to)
                throw new ArgumentException("The arg from should not be greater than the arg to.");
            return from >= MinExist && to <= MaxExist;
        }
        public bool IsExist(PositionRange range) => IsExist(range.GetTiles(ExistTileSize).ToHashSet());
        /// <summary>
        /// 此处传入的Tile，其Size应为ExistTileSize，否则无效
        /// </summary>
        public bool IsExist(HashSet<Tile> tiles) => TilesExist.Overlaps(tiles);
    }
}
