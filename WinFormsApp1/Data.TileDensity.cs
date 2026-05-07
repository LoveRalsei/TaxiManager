using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public class TileDensity
    {
        private readonly static Dictionary<(Tile tile, DateTime time), uint> _densityMap = [];
        private static Task? _task;
        public static bool Loaded => _task?.IsCompleted ?? false;
        public static bool IsError => _task?.IsFaulted ?? false;
        public static Exception? Error => _task?.Exception;
        public static void Init()
        {
            _task = Task.Run(() =>
            {
                var drivers = DataLoader.Drivers;
                foreach (var driver in drivers)
                {
                    foreach (var node in driver.Nodes)
                    {
                        //if (_densityMap.TryGetValue((node.Position.GetTile())))
                    }
                }
            });
        }
    }
}
