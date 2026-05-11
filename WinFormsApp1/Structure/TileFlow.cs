using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.Structure
{
    public class TileFlow
    {
        private static Task? _task;
        private static readonly Dictionary<Driver, Dictionary<(Tile tile, int unit), HashSet<Tile>>> _flowRawMap = [];
        /// <summary>
        /// 二层字典 Key 为 起点 Tile 的 Map
        /// </summary>
        private static readonly Dictionary<int, Dictionary<Tile, Dictionary<Tile, int>>> _flowFromKey = [];
        /// <summary>
        /// 二层字典 Key 为 终点 Tile 的 Map
        /// </summary>
        private static readonly Dictionary<int, Dictionary<Tile, Dictionary<Tile, int>>> _flowToKey = [];

        public static bool Loaded => _task?.IsCompleted ?? false;

        /// <summary>
        /// 获取从起点到终点直线经过的所有大小为1的瓦片（不包括起点）
        /// </summary>
        private static HashSet<Tile> GetTilesOnLine(Position from, Position to)
        {
            var tiles = new HashSet<Tile>();
            Position curr = from;
            int tileSize = 1; // 大小为1的瓦片 = 100米

            // 方向步长
            int stepX = to.X > from.X ? 1 : (to.X < from.X ? -1 : 0);
            int stepY = to.Y > from.Y ? 1 : (to.Y < from.Y ? -1 : 0);

            Tile currTile = from.GetTile((byte)tileSize);

            while (true)
            {
                // 计算到下一个X边界和Y边界的距离(以t为比例)
                double tX = double.MaxValue, tY = double.MaxValue;

                if (stepX != 0)
                {
                    // 下一个X边界距离（归一化）
                    uint tileX = currTile.X;
                    double nextBoundaryX = (stepX > 0 ? (tileX + 1) * 100.0 : tileX * 100.0) + Position.MinX;
                    tX = (nextBoundaryX - curr.X) / (stepX * (double)(to.X - from.X));
                }

                if (stepY != 0)
                {
                    // 下一个Y边界距离（归一化）
                    uint tileY = currTile.Y;
                    double nextBoundaryY = (stepY > 0 ? (tileY + 1) * 100.0 : tileY * 100.0) + Position.MinY;
                    tY = (nextBoundaryY - curr.Y) / (stepY * (double)(to.Y - from.Y));
                }

                // 选择最近的边界前进
                if (tX < tY)
                {
                    curr = Position.From((uint)(curr.X + stepX * (to.X - from.X) * tX),
                                         (uint)(curr.Y + stepY * (to.Y - from.Y) * tX));
                    currTile = new Tile((byte)tileSize, currTile.X + (uint)stepX, currTile.Y);
                }
                else
                {
                    curr = Position.From((uint)(curr.X + stepX * (to.X - from.X) * tY),
                                         (uint)(curr.Y + stepY * (to.Y - from.Y) * tY));
                    currTile = new Tile((byte)tileSize, currTile.X, currTile.Y + (uint)stepY);
                }

                tiles.Add(currTile);

                // 到达终点则停止
                if ((stepX >= 0 && curr.X >= to.X) || (stepX <= 0 && curr.X <= to.X))
                    if ((stepY >= 0 && curr.Y >= to.Y) || (stepY <= 0 && curr.Y <= to.Y))
                        break;
            }

            return tiles;
        }
        public static void Initialize()
        {
            _task = DataLoader.ExecuteAfterLoaded(() =>
            {
                foreach (var driver in DataLoader.Drivers)
                {
                    Dictionary<(Tile tile, int unit), HashSet<Tile>> movements = [];
                    Position? lastPosition = null;
                    int lastUnit = 0;
                    for (var time = driver.MinExist; time <= driver.MaxExist; time += TimeUnit.UnitTime)
                    {
                        var currPosition = driver.GetPosition(time);
                        if (currPosition == null)
                        {
                            lastPosition = null;
                            lastUnit = 0;
                            continue;
                        }

                        int currUnit = TimeUnit.GetUnit(time);
                        if (lastPosition != null)
                        {
                            Tile from = lastPosition.Value.GetTile();
                            Tile to = currPosition.Value.GetTile();
                            if (from == to)
                                goto breakIf;
                            var passed = GetTilesOnLine(lastPosition.Value, currPosition.Value);
                            if (passed.Count > 0)
                                movements.Add((from, lastUnit), passed);
                        }

                        breakIf: ;
                        lastPosition = currPosition;
                        lastUnit = currUnit;
                    }

                    _flowRawMap.Add(driver, movements);
                    // 处理后添加到 _flowMap
                    foreach (var entry in movements)
                    {
                        var fromTile = entry.Key.tile;
                        var unit = entry.Key.unit;
                        if (!_flowFromKey.TryGetValue(unit, out var flowFromMap))
                        {
                            flowFromMap = [];
                            _flowFromKey.Add(entry.Key.unit, flowFromMap);
                        }

                        if (!_flowToKey.TryGetValue(unit, out var flowToMap))
                        {
                            flowToMap = [];
                            _flowToKey.Add(entry.Key.unit, flowToMap);
                        }

                        if (!flowFromMap.TryGetValue(fromTile, out var flowFrom))
                        {
                            flowFrom = [];
                            flowFromMap.Add(fromTile, flowFrom);
                        }

                        foreach (var toTile in entry.Value)
                        {
                            var countFrom = flowFrom.GetValueOrDefault(toTile, 0);
                            flowFrom[toTile] = countFrom + 1;

                            if (!flowToMap.TryGetValue(toTile, out var flowTo))
                            {
                                flowTo = [];
                                flowToMap.Add(toTile, flowTo);
                            }
                            var countTo = flowTo.GetValueOrDefault(fromTile, 0);
                            flowTo[fromTile] = countTo + 1;
                            
                        }
                    }
                }
            });
        }

        public static Dictionary<Tile, int> GetFlowFrom(Tile from, int timeUnit)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据未完成预处理");
                _task?.Wait();
            }

            return 
                _flowFromKey.TryGetValue(timeUnit, out var flowFromMap) ? 
                    flowFromMap.TryGetValue(from, out var flowFrom) ? flowFrom : [] : [];
        }

        public static Dictionary<Tile, int> GetFlowTo(Tile to, int timeUnit)
        {
            if (!Loaded)
            {
                MessageBox.Show("数据未完成预处理");
                _task?.Wait();
            }
            
            return 
                _flowToKey.TryGetValue(timeUnit, out var flowToMap) ? 
                    flowToMap.TryGetValue(to, out var flowTo) ? flowTo : [] : [];
        }
    }
}
