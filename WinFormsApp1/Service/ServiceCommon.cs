using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiManager.Structure;

namespace TaxiManager.Service
{
    public class ServiceCommon : IServiceCommon
    {
        public static readonly ServiceCommon Instance = new();

        DateTime IServiceCommon.GetMaxTime() => DataLoader.TimeMax;

        DateTime IServiceCommon.GetMinTime() => DataLoader.TimeMin;

        /// <summary>
        /// 判断线段是否与矩形区域相交
        /// </summary>
        /// <param name="start">线段起点</param>
        /// <param name="end">线段终点</param>
        /// <param name="range">矩形区域</param>
        /// <returns>是否相交</returns>
        public bool IsLineIntersectRange(Position start, Position end, PositionRange range)
        {
            // 如果线段的任一端点在矩形内，则认为相交
            if (range.IsIn(start) || range.IsIn(end))
                return true;

            // 检查线段是否与矩形的四条边相交
            var corners = range.Corners;
            for (int i = 0; i < 4; i++)
            {
                var p1 = corners[i];
                var p2 = corners[(i + 1) % 4];
                if (IsLinesIntersect(start, end, p1, p2))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判断两条线段是否相交
        /// </summary>
        private bool IsLinesIntersect(Position p1, Position p2, Position p3, Position p4)
        {
            // 使用向量叉积判断线段相交
            double d1 = CrossProduct(p3, p4, p1);
            double d2 = CrossProduct(p3, p4, p2);
            double d3 = CrossProduct(p1, p2, p3);
            double d4 = CrossProduct(p1, p2, p4);

            // 如果两个端点分别在另一条线段的两侧，则相交
            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
                return true;

            // 处理共线情况
            if (d1 == 0 && IsPointOnSegment(p3, p4, p1)) return true;
            if (d2 == 0 && IsPointOnSegment(p3, p4, p2)) return true;
            if (d3 == 0 && IsPointOnSegment(p1, p2, p3)) return true;
            if (d4 == 0 && IsPointOnSegment(p1, p2, p4)) return true;

            return false;
        }

        /// <summary>
        /// 计算向量叉积
        /// </summary>
        private double CrossProduct(Position a, Position b, Position c)
        {
            return (long)(b.X - a.X) * (long)(c.Y - a.Y) - (long)(b.Y - a.Y) * (long)(c.X - a.X);
        }

        /// <summary>
        /// 判断点c是否在线段ab上
        /// </summary>
        private bool IsPointOnSegment(Position a, Position b, Position c)
        {
            // 首先检查c是否在ab的包围盒内
            if (c.X < Math.Min(a.X, b.X) || c.X > Math.Max(a.X, b.X) ||
                c.Y < Math.Min(a.Y, b.Y) || c.Y > Math.Max(a.Y, b.Y))
                return false;

            // 然后检查叉积是否为0（三点共线）
            return CrossProduct(a, b, c) == 0;
        }

        /// <summary>
        /// 计算线段与矩形区域的交点参数t值（0-1之间）
        /// </summary>
        /// <param name="start">线段起点</param>
        /// <param name="end">线段终点</param>
        /// <param name="range">矩形区域</param>
        /// <returns>交点的参数t值列表，按从小到大排序</returns>
        public List<double> GetLineRangeIntersectionParameters(Position start, Position end, PositionRange range)
        {
            var intersections = new List<double>();

            // 检查线段端点是否在矩形内
            if (range.IsIn(start))
                intersections.Add(0.0);
            if (range.IsIn(end))
                intersections.Add(1.0);

            // 检查线段与矩形四边的交点
            var corners = range.Corners;
            for (int i = 0; i < 4; i++)
            {
                var p1 = corners[i];
                var p2 = corners[(i + 1) % 4];
                
                var t = GetLineSegmentIntersectionParameter(start, end, p1, p2);
                if (t.HasValue && t.Value >= 0 && t.Value <= 1)
                {
                    intersections.Add(t.Value);
                }
            }

            // 去重并排序
            return intersections.Distinct().OrderBy(t => t).ToList();
        }

        /// <summary>
        /// 计算两条线段的交点参数t值（相对于第一条线段）
        /// </summary>
        /// <returns>交点在第一条线段上的参数t值，如果没有交点则返回null</returns>
        private double? GetLineSegmentIntersectionParameter(Position p1, Position p2, Position p3, Position p4)
        {
            long denom = (long)(p1.Y - p2.Y) * (long)(p4.X - p3.X) - (long)(p1.X - p2.X) * (long)(p4.Y - p3.Y);
            if (denom == 0)
                return null; // 平行线

            double t = ((long)(p1.X - p3.X) * (long)(p4.Y - p3.Y) - (long)(p1.Y - p3.Y) * (long)(p4.X - p3.X)) / (double)denom;
            
            // 检查交点是否在第二条线段上
            double u = -((long)(p1.Y - p2.Y) * (long)(p1.X - p3.X) - (long)(p1.X - p2.X) * (long)(p1.Y - p3.Y)) / (double)denom;
            
            if (u >= 0 && u <= 1)
                return t;
            
            return null;
        }

        /// <summary>
        /// 判断从Position a到Position b的有向线段是否经过两个PositionRange范围，以及经过的顺序
        /// </summary>
        /// <param name="a">线段起点</param>
        /// <param name="b">线段终点</param>
        /// <param name="range1">第一个位置范围</param>
        /// <param name="range2">第二个位置范围</param>
        /// <returns>
        /// 返回值说明：
        /// - 0: 线段不经过任何一个范围
        /// - 1: 只经过range1
        /// - 2: 只经过range2
        /// - 3: 先经过range1，再经过range2
        /// - 4: 先经过range2，再经过range1
        /// </returns>
        public int CheckLinePassThroughTwoRanges(Position a, Position b, PositionRange range1, PositionRange range2)
        {
            // 获取与两个范围的交点参数
            var intersections1 = GetLineRangeIntersectionParameters(a, b, range1);
            var intersections2 = GetLineRangeIntersectionParameters(a, b, range2);

            bool passesRange1 = intersections1.Count > 0;
            bool passesRange2 = intersections2.Count > 0;

            if (!passesRange1 && !passesRange2)
                return 0; // 不经过任何范围

            if (passesRange1 && !passesRange2)
                return 1; // 只经过range1

            if (!passesRange1 && passesRange2)
                return 2; // 只经过range2

            // 都经过，需要判断顺序
            // 取每个范围的第一个交点参数进行比较
            double firstT1 = intersections1.First();
            double firstT2 = intersections2.First();

            if (firstT1 < firstT2)
                return 3; // 先经过range1，再经过range2
            if (firstT2 < firstT1)
                return 4; // 先经过range2，再经过range1
            // 交点参数相同，可能是相邻或重叠的情况
            // 这里简化处理，认为同时经过
            return 3; // 默认返回12，可根据具体需求调整
            
        }

        public Color GetHotColor(float percent, float yellowPercent = 0.5f)
        {
            if (percent <= 0)
                return Color.FromArgb(0x7f00ff2f);
            if (percent >= 1)
                return Color.FromArgb(0x7fff002f);
            if (percent <= yellowPercent)
                return Color.FromArgb(0x7f00ff2f | (((int)(0xff * percent / yellowPercent)) << 16));
            return Color.FromArgb(0x7fff002f | (((int)(0xff * (1 - percent) / (1 - yellowPercent))) << 8));
        }

        public List<Tile> GetTilesOnLine(byte tileSize, Position from, Position to, int ignoreArguments = 0)
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
    }
}
