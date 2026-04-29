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
    public class DataLoader
    {
        // 约 5 米的容忍度，用于修正GPS误差和压缩不动的节点。
        public const double DistanceTolerance = 5e-5;

        private static Driver[] _drivers = [];
        public static Driver[] Drivers
        {
            get
            {
                // 未完成加载时，等待加载完成
                _loadTask?.Wait();
                if (Error != null)
                    throw Error;
                return _drivers;
            }
        }
        private static int _rawDriversCount;
        public static int RawDriversCount => _rawDriversCount;
        private static int _driversCount = 0;
        public static int DriversCount => _driversCount;
        private static DateTime _timeMin = DateTime.MinValue;
        private static DateTime _timeMax = DateTime.MaxValue;
        public static DateTime TimeMin => _timeMin;
        public static DateTime TimeMax => _timeMax;

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
                    var driver = new Driver(() =>
                    {
                        // 较宽松地预估长度
                        List<PathNode> nodes = [];
                        using (StreamReader reader = new(stream))
                        {
                            const double dTolerance = DistanceTolerance;
                            double lastLongitude = 0, lastLatitude = 0;
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

                                if (!double.TryParse(xSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude) ||
                                    !double.TryParse(ySpan, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude))
                                {
                                    continue;
                                }
                                if (!Position.IsValid(longitude, latitude))
                                    continue;
                                // 如果当前节点和上一个节点的位置相差大于容忍度，则认为是新的有效节点。
                                if (Math.Abs(longitude - lastLongitude) >= dTolerance || Math.Abs(latitude - lastLatitude) >= dTolerance)
                                {
                                    // 中间有节点被略过，需要插入一个节点。
                                    if (ignoredTime != null)
                                    {
                                        var tailNode = new PathNode(ignoredTime.Value, Position.FromRaw(lastLongitude, lastLatitude));
                                        nodes.Add(tailNode);
                                    }
                                    lastLongitude = longitude;
                                    lastLatitude = latitude;
                                    ignoredTime = null;
                                    var node = new PathNode(date, Position.FromRaw(longitude, latitude));
                                    nodes.Add(node);
                                }
                                else
                                {
                                    ignoredTime = date;
                                }
                            }
                        }
                        // 除去多余项
                        nodes.TrimExcess();
                        return nodes;
                    });
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
                    List<Task<Driver?>> entryLoaders = [];
                    foreach (var zipFile in zipFiles)
                    {
                        using var zip = ZipFile.OpenRead(zipFile);
                        foreach (var entry in zip.Entries)
                        {
                            var entryStream = new MemoryStream((int)entry.Length);
                            using var rawEntryStream = entry.Open();
                            rawEntryStream.CopyTo(entryStream);
                            entryStream.Position = 0;
                            entryLoaders.Add(Task.Run(_getEntryStreamHandler(entryStream)));
                        }
                    }
                    _loadDiskMs = (long)(DateTime.Now - firstTime).TotalMilliseconds;
                    Task.WhenAll(entryLoaders).Wait();
                    int index = 0;
                    DateTime minTime = DateTime.MaxValue, maxTime = DateTime.MinValue;
                    foreach (var task in entryLoaders)
                    {
                        var driver = task.Result;
                        if (driver == null) continue;
                        driver.Id = ++index;
                        if (driver.Nodes.Count == 0)
                            continue;
                        if (driver.Nodes.First().Time < minTime)
                            minTime = driver.Nodes.First().Time;
                        if (driver.Nodes.Last().Time > maxTime)
                            maxTime = driver.Nodes.Last().Time;
                        drivers.Add(driver);
                    }
                    // 转换为数组
                    _drivers = drivers.ToArray();
                    if (minTime == DateTime.MaxValue || maxTime == DateTime.MinValue)
                        throw new Exception("Does not have valid node.");
                    _timeMin = minTime;
                    _timeMax = maxTime;
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
