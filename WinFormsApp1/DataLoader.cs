using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        public int Id;
        public readonly List<PathNode> Nodes = [];
        public bool IsEmpty => Nodes.Count == 0;
        public Driver(List<PathNode> nodes) 
        {
            Nodes = nodes;
        }
        public Driver(PathSupplier pathSupplier) : this(pathSupplier()) { }
        public Driver(Stream stream) : this(new PathSupplier(() =>
        {
            // 较宽松地预估长度
            List<PathNode> nodes = [];
            using (StreamReader reader = new(stream))
            {
                const double dTolerance = 0.00005;// 约 5 米的容忍度，用于修正GPS误差和压缩不动的节点。
                double lastX = 0, lastY = 0;
                DateTime? ignoredTime = null;
                for (var line = reader.ReadLine(); line != null && line.Length > 0; line = reader.ReadLine())
                {
                    // 使用 Span 避免字符串分配
                    ReadOnlySpan<char> lineSpan = line.AsSpan();

                    // 手动解析 CSV，避免 Split 的数组分配
                    int firstComma = lineSpan.IndexOf(',');
                    if (firstComma < 0) continue;

                    int secondComma = lineSpan.Slice(firstComma + 1).IndexOf(',');
                    if (secondComma < 0) continue;
                    secondComma += firstComma + 1;

                    int thirdComma = lineSpan.Slice(secondComma + 1).IndexOf(',');
                    if (thirdComma < 0) continue;
                    thirdComma += secondComma + 1;

                    // 解析日期
                    ReadOnlySpan<char> dateSpan = lineSpan.Slice(firstComma + 1, secondComma - firstComma - 1);
                    if (!DateTime.TryParseExact(dateSpan, PathNode.DateFormat,
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    {
                        continue;
                    }

                    // 解析坐标
                    ReadOnlySpan<char> xSpan = lineSpan.Slice(secondComma + 1, thirdComma - secondComma - 1);
                    ReadOnlySpan<char> ySpan = lineSpan.Slice(thirdComma + 1);

                    if (!double.TryParse(xSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out double x) ||
                        !double.TryParse(ySpan, NumberStyles.Float, CultureInfo.InvariantCulture, out double y))
                    {
                        continue;
                    }
                    if (!Position.IsValid(x, y))
                        continue;
                    // 如果当前节点和上一个节点的位置相差大于容忍度，则认为是新的有效节点。
                    if (Math.Abs(x - lastX) >= dTolerance || Math.Abs(y - lastY) >= dTolerance)
                    {
                        // 中间有节点被略过，需要插入一个节点。
                        if (ignoredTime != null)
                        {
                            var tailNode = new PathNode(ignoredTime.Value, lastX, lastY);
                            nodes.Add(tailNode);
                        }
                        lastX = x;
                        lastY = y;
                        var node = new PathNode(date, x, y);
                        nodes.Add(node);
                    } else
                    {
                        ignoredTime = date;
                    }
                }
            }
            // 除去多余项
            nodes.TrimExcess();
            return nodes;
        })) { }

        /// <summary>
        /// 默认时间误差为15分钟。
        /// 获取指定时间点的位置信息，并容许一定的误差，误差范围内的位置信息将被线性插值处理。
        /// 如果不存在误差内的位置信息，则返回null。
        /// </summary>
        public Position? GetPosition(DateTime time)
        {
            return GetPosition(time, TimeTolerance.Default);
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
        public bool IsValid() => IsValid(this);
        public static bool IsValid(Position? position) => position != null && IsValid(position.Value.X, position.Value.Y);
        public static bool IsValid(double x, double y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;
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
        private static int _rawDriversCount;
        public static int RawDriversCount => _rawDriversCount;
        private static int _driversCount = 0;
        public static int DriversCount => _driversCount;
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

        private static long _loadDiskMs = 0;
        public static long LoadDiskMs => _loadDiskMs;
        private static long _loadTotalMs = 0;
        public static long LoadTotalMs => _loadTotalMs; 

        private static Func<Driver?> _getEntryStreamHandler(Stream stream)
        {
            return () =>
            {
                try
                {
                    var driver = new Driver(stream);
                    if (!driver.IsEmpty)
                        Interlocked.Increment(ref _driversCount);
                    else
                        driver = null;
                    // 原子操作计数
                    Interlocked.Increment(ref _loadedCount);
                    return driver;
                }
                catch (Exception ex)
                {
                    _error = ex;
                }
                finally
                {
                    stream.Dispose();
                }
                return null;
            };
        }

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
            _rawDriversCount = totalEntries;

            // 开独立线程用于加载数据，避免阻塞UI线程
            _loadTask = Task.Run(() =>
            {
                try
                {
                    var firstTime = DateTime.Now;
                    List<Driver> drivers = [];
                    LinkedList<Task<Driver?>> entryLoaders = [];
                    var zipFiles = Directory.GetFiles(_dataPath, "*.zip");
                    foreach (var zipFile in zipFiles)
                    {
                        using var zip = ZipFile.OpenRead(zipFile);
                        foreach (var entry in zip.Entries)
                        {
                            var entryStream = new MemoryStream();
                            using var rawEntryStream = entry.Open();
                            rawEntryStream.CopyTo(entryStream);
                            entryStream.Position = 0;
                            entryLoaders.AddLast(Task.Run(_getEntryStreamHandler(entryStream)));
                        }
                    }
                    _loadDiskMs = (long)(DateTime.Now - firstTime).TotalMilliseconds;
                    Task.WhenAll(entryLoaders);
                    int index = 0;
                    foreach (var task in entryLoaders)
                    {
                        if (task.Result == null)
                            continue;
                        index++;
                        task.Result.Id = index;
                        drivers.Add(task.Result);
                    }
                    // 转换为数组
                    _drivers = drivers.ToArray();
                    _loadTotalMs = (long)(DateTime.Now - firstTime).TotalMilliseconds;
                }
                catch (Exception error)
                {
                    _error = error;
                }
            });
        }
    }
}
