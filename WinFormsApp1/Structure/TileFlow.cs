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
        private static readonly Dictionary<Driver, Dictionary<(Tile tile, int unit), HashSet<Tile>>> _flowMap = [];
        private static readonly Dictionary<(Tile tile, int unit), HashSet<Tile>> _emptyMap = [];
        private static readonly HashSet<Tile> _emptySet = [];

        /// <summary>
        /// 获取从起点到终点直线经过的所有大小为1的瓦片（不包括起点）
        /// </summary>
        public static HashSet<Tile> GetTilesOnLine(Position from, Position to)
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
                List<Task<(Driver driver, Dictionary<(Tile tile, int unit), HashSet<Tile>> map)>> tasks = [];
                foreach (var driver in DataLoader.Drivers)
                {
                    tasks.Add(Task.Run(() =>
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
                            breakIf:;
                            lastPosition = currPosition;
                            lastUnit = currUnit;
                        }
                        return (driver, movements);
                    }));
                }
                Task.WhenAll(tasks).Wait();
                foreach (var task in tasks)
                {
                    _flowMap.Add(task.Result.driver, task.Result.map);
                }
            });
        }
    }
}
