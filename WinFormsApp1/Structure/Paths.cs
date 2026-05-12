using System.Diagnostics;

namespace TaxiManager.Structure;

public class Paths
{
    private static Task? _task;
    public static bool Loaded => _task?.IsCompleted ?? false;
    
    private static Dictionary<Tile, float>? _flowsTotal;
    public static Dictionary<Tile, float> FlowsTotal
    {
        get
        {
            if (Loaded) return _flowsTotal!;
            MessageBox.Show("尚未完成路径数据预处理");
            _task?.Wait();
            return _flowsTotal!;
        }
    }

    public static void Initialize()
    {
        _task = Density.ExecuteAfterLoaded(() =>
        {
            Console.WriteLine("Initializing Paths");
            var sw = Stopwatch.StartNew();
            int startUnit = TimeUnit.GetUnit(DataLoader.TimeMin);
            int endUnit = TimeUnit.GetUnit(DataLoader.TimeMax);

            Dictionary<Tile, float> flowsTotal = [];
            
            Dictionary<Tile, float> currDensityMap = Density.GetDensity(3, startUnit);
            Dictionary<Tile, float> nextDensityMap = [];
            for (var unit = startUnit; unit < endUnit; unit++, currDensityMap = nextDensityMap)
            {
                nextDensityMap = Density.GetDensity(3, unit + 1);
                foreach (var (tile, density) in currDensityMap)
                {
                    if (!nextDensityMap.TryGetValue(tile, out var densityNext)) continue;
                    var delta = Math.Abs(density - densityNext);
                    flowsTotal[tile] = flowsTotal.GetValueOrDefault(tile) + delta;
                }
            }
            _flowsTotal = flowsTotal;
            sw.Stop();
            Console.WriteLine($"Flows Initialized in {sw.ElapsedMilliseconds}ms");
        });
    }
}