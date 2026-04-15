using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TaxiManager
{
    public readonly struct TimeTolerance
    {
        // 15 minutes tolerance by default.
        public const long Default = 1000 * 60 * 15;
        public readonly long MillisecondsTolerance;
        public TimeTolerance(long tolerance) : this() { MillisecondsTolerance = tolerance; }
        public static TimeTolerance Make(long tolerance) => new(tolerance);
        public static TimeTolerance Seconds(long tolerance) => Make(tolerance * 1000);
        public static TimeTolerance Minutes(long tolerance) => Seconds(60 * tolerance);
        public static TimeTolerance Hours(long tolerance) => Minutes(60 * tolerance);
        public static TimeTolerance Days(long tolerance) => Hours(24 * tolerance);
        public override string ToString() => $"{MillisecondsTolerance}ms";
        public static implicit operator long(TimeTolerance tolerance) => tolerance.MillisecondsTolerance;
        public static implicit operator TimeTolerance(long tolerance) => Make(tolerance);
    }
    public class Driver
    {
        public readonly int Id;
        public readonly List<PathNode> Nodes = [];
        public bool IsEmpty => Nodes.Count == 0;
        public Driver(int id, List<PathNode> nodes) 
        {
            Id = id;
            Nodes = nodes;
        }
        public Driver(int id, PathSupplier pathSupplier) : this(id, pathSupplier()) { }
        public Driver(ZipArchiveEntry entry) : this(int.Parse(Path.GetFileNameWithoutExtension(entry.Name)), new PathSupplier(() =>
        {
            List<PathNode> nodes = [];
            using (var stream = entry.Open())
            using (StreamReader reader = new(stream))
            {
                for (var line = reader.ReadLine(); line != null && line.Length > 0; line = reader.ReadLine())
                {
                    var splited = line.Split(',');
                    var date = DateTime.ParseExact(splited[1], PathNode.DateFormat, CultureInfo.InvariantCulture);
                    var x = double.Parse(splited[2]);
                    var y = double.Parse(splited[3]);
                    var node = new PathNode(date, x, y);
                    if (node.IsValid())
                        nodes.Add(node);
                }
            }
            return nodes;
        })) { }

        /// <summary>
        /// 默认时间误差为15分钟。
        /// 获取指定时间点的位置信息，并容许一定的误差，误差范围内的位置信息将被线性插值处理。
        /// 如果不存在误差内的位置信息，则返回null。
        /// </summary>
        public Position? GetPosition(DateTime time)
        {
            return getPosition(time, TimeTolerance.Default);
        }

        /// <summary>
        /// 获取指定时间点的位置信息，并容许一定的误差，误差范围内的位置信息将被线性插值处理。
        /// 如果不存在误差内的位置信息，则返回null。
        /// </summary>
        public Position? GetPosition(DateTime time, TimeTolerance tolerance)
        {
            if (IsEmpty) return null;

            // 二分查找找到第一个 >= time 的节点
            int left = 0;
            int right = Nodes.Count - 1;
            int index = right;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (Nodes[mid].Date < time)
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
                long diffRight = (long)(rightNode.Date - time).TotalMilliseconds;
                if (diffRight <= toleranceMs)
                {
                    // 如果正好在目标时间点上
                    if (diffRight == 0) return rightNode.Position;

                    // 如果左侧有节点，尝试插值
                    if (index > 0)
                    {
                        var leftNode = Nodes[index - 1];
                        long diffLeft = (long)(time - leftNode.Date).TotalMilliseconds;
                        if (diffLeft <= toleranceMs)
                        {
                            float scale = (float)(diffLeft * 1.0 / (diffLeft + diffRight));
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
                long diffLeft = (long)(time - leftNode.Date).TotalMilliseconds;
                if (diffLeft <= toleranceMs)
                {
                    return leftNode.Position;
                }
            }

            // 左右节点都无效，返回null
            return null;
        }
    }
    public delegate List<PathNode> PathSupplier();
    public readonly struct Position
    {
        public const double MinX = 110, MaxX = 125, MinY = 30, MaxY = 50;
        public readonly double X;
        public readonly double Y;
        public Position(double x, double y) : this()
        {
            X = x;
            Y = y;
        }
        public Position? Lerp(Position? target, float scale)
        {
            return Lerp(this, target, scale);
        }
        public bool IsValid() => X >= MinX && X <= MaxX && Y >= MinY && Y <= MaxY;
        public static Position? Lerp(Position? from, Position? to, float scale)
        {
            if (from == null && to == null) return null;
            if (from == null) return to;
            if (to == null) return from;
            return Position.Make(
                from.Value.X + (to.Value.X - from.Value.X) * scale,
                from.Value.Y + (to.Value.Y - from.Value.Y) * scale
                );
        }
        public static Position Make(double x, double y) => new(x, y);
    }
    public readonly struct PathNode
    {
        public static readonly string DateFormat = new("yyyy-MM-dd HH:mm:ss");
        public readonly DateTime Time;
        public readonly Position Position;

        public PathNode(DateTime date, double x, double y) : this()
        {
            Date = date;
            Position = Position.Make(x, y);
        }

        public bool IsValid() => Position.IsValid();

        public DateTime Date { get; }
    }
    public class DataLoader
    {
        private static Driver[] _drivers = [];
        public static Driver[] Drivers
        {
            get
            {
                // 未完成加载时，等待加载完成
                while (!Loaded) { }
                if (Error != null)
                    throw Error;
                return _drivers;
            }
        }
        public static int DriversCount => _drivers.Length;
        private static int _loadedCount;
        public static int LoadedCount => _loadedCount;
        private static Exception? _error;
        public static Exception? Error => _error;
        public static bool Loaded
        {
            get
            {
                return _loadTask?.IsCompleted ?? false;
            }
        }
        public static bool IsError
        {
            get
            {
                return Error != null;
            }
        }
        private static readonly string _dataPath = Path.Combine(AppContext.BaseDirectory, "data");

        private static Task? _loadTask;

        public static void Load()
        {
            if (Loaded)
                throw new Exception("Data has already been loaded.");

            // 获取所有zip文件
            var zipFiles = Directory.GetFiles(_dataPath, "*.zip");
            if (zipFiles.Length == 0)
                throw new Exception($"No zip files found in {_dataPath}");

            // 预计算总条目数
            int totalEntries = 0;
            foreach (var zipFile in zipFiles)
            {
                using var zip = ZipFile.OpenRead(zipFile);
                totalEntries += zip.Entries.Count;
            }
            _drivers = new Driver[totalEntries];

            // 开独立线程用于加载数据，避免阻塞UI线程
            _loadTask = Task.Run(() =>
            {
                try
                {
                    var zipFiles = Directory.GetFiles(_dataPath, "*.zip");
                    foreach (var zipFile in zipFiles)
                    {
                        using var zip = ZipFile.OpenRead(zipFile);
                        foreach (var entry in zip.Entries)
                        {
                            var driver = new Driver(entry);
                            _drivers[driver.Id - 1] = driver;
                            _loadedCount++;
                        }
                    }
                }
                catch (Exception error)
                {
                    _error = error;
                }
            });
        }
    }
}
