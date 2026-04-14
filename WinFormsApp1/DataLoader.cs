using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    
    public class Driver
    {
        public readonly int Id;
        public readonly List<PathNode> Nodes = [];
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
                    //var idInFileName = int.Parse(Path.GetFileNameWithoutExtension(filePath));
                    //var idInFile = int.Parse(splited[0]);
                    //if (idInFileName != idInFile)
                    //    throw new Exception("The name id ."+ idInFileName + ". is different with data id ."+ idInFile +".");
                    var date = DateTime.ParseExact(splited[1], PathNode.DateFormat, CultureInfo.InvariantCulture);
                    var x = double.Parse(splited[2]);
                    var y = double.Parse(splited[3]);
                    nodes.Add(new PathNode(date, x, y));
                }
            }
            return nodes;
        })) { }
    }
    public delegate List<PathNode> PathSupplier();
    public readonly struct PathNode
    {
        public static readonly string DateFormat = new("yyyy-MM-dd HH:mm:ss");
        readonly DateTime Time;
        readonly double X;
        readonly double Y;

        public PathNode(DateTime date, double x, double y) : this()
        {
            Date = date;
            X = x;
            Y = y;
        }

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
