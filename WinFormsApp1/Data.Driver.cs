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
    public class Driver(List<PathNode> nodes)
    {
        public int Id;
        public readonly List<PathNode> Nodes = nodes;
        public bool IsEmpty => Nodes.Count == 0;

        public Driver(PathSupplier pathSupplier) : this(pathSupplier()) { }
        
        /// <summary>
        /// 默认时间误差为15分钟。
        /// 获取指定时间点的位置信息，并容许一定的误差，误差范围内的位置信息将被线性插值处理。
        /// 如果不存在误差内的位置信息，则返回null。
        /// </summary>
        public Position? GetPosition(DateTime time, TimeTolerance tolerance = default)
        {
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
                    if (diffRight == 0) return rightNode.Position;

                    // 如果左侧有节点，尝试插值
                    if (index > 0)
                    {
                        var leftNode = Nodes[index - 1];
                        long diff = (long)(rightNode.Time - leftNode.Time).TotalMilliseconds;
                        if (diff <= toleranceMs)
                        {
                            long diffLeft = (long)(time - leftNode.Time).TotalMilliseconds;
                            float scale = (float)(diffLeft * 1.0 / diff);
                            return Position.Lerp(leftNode.Position, rightNode.Position, scale);
                        }
                    }

                    // 无法插值但右侧在容差内
                    return rightNode.Position;
                }
            }

            // 如果检测右侧节点时未返回值，说明右侧节点无效，只需要再检查左侧节点
            if (index > 0)
            {
                var leftNode = Nodes[index - 1];
                long diffLeft = (long)(time - leftNode.Time).TotalMilliseconds;
                if (diffLeft <= toleranceMs)
                {
                    return leftNode.Position;
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
    }
}
