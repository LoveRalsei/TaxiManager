using System.Diagnostics;
using System.Runtime.InteropServices;
using TaxiManager.Service;

namespace TaxiManager.Structure;

public class Speeds
{
    public const byte AnalyzeTileSize = 3;
    /// <summary>
    /// 最大速度的平方 单位 (km/h)^2
    /// </summary>
    public const double MaxSpeedSquared = 400 * 400;
    private static Task? _task;
    public static bool Loaded => _task?.IsCompleted ?? false;
    
    private static readonly Dictionary<Tile, float>[] _speedsMap = new Dictionary<Tile, float>[24];
    
    public static void Initialize()
    {
        _task = Flows.ExecuteAfterLoaded(() =>
        {
            Console.WriteLine("Initializing Speeds");
            var sw = Stopwatch.StartNew();
            var sw2 = Stopwatch.StartNew();
            var service = IServiceCommon.Instance;

            var speedsRawMap = new Dictionary<Tile, (double totalSpeed, int count)>[24];
            for (int i = 0; i < 24; i++)
                speedsRawMap[i] = [];
            
            var minTime = DataLoader.TimeMin;
            var maxTime = DataLoader.TimeMax;
            var timeStep = TimeUnit.UnitTime;
            for (DateTime time = minTime, nextTime = time + timeStep;
                 time <= maxTime;
                 time = nextTime, nextTime += timeStep)
            {
                var index = time.Hour;
                var unit = TimeUnit.GetUnit(time);
                var unitNext = TimeUnit.GetNextUnit(unit);
                var positions = Flows.GetPositions(unit);
                var positionsNext = Flows.GetPositions(unitNext);
                var currMap = speedsRawMap[index];
                
                foreach (var (driver, posFrom) in positions)
                {
                    if (!positionsNext.TryGetValue(driver, out var posTo)) continue;
                    if (posFrom.GetTile(AnalyzeTileSize) == posTo.GetTile(AnalyzeTileSize)) continue;
                    // 获得时速的平方
                    var speed = posFrom.DistanceSquaredTo(posTo) * 16 * 1e-6;
                    // 速度明显异常(<=0或>400km/h)，忽略
                    if (speed is <= 0 or > MaxSpeedSquared) continue;
                    speed = Math.Sqrt(speed);
                    
                    service.ForTilesOnLine(AnalyzeTileSize, posFrom, posTo, 0, (tile) =>
                    {
                        ref var data = ref CollectionsMarshal.GetValueRefOrAddDefault(currMap, tile, out var exists);
                        data.totalSpeed += speed;
                        data.count++;
                    });
                }
            }
            sw2.Stop();
            Console.WriteLine($"Speeds Raw Map Initialized in {sw2.ElapsedMilliseconds}ms");
            for (int i = 0; i < 24; i++)
            {
                _speedsMap[i] = speedsRawMap[i].Select((pair) => (pair.Key, (float)(pair.Value.totalSpeed / pair.Value.count))).ToDictionary();
            }
            sw.Stop();
            Console.WriteLine($"Speeds Initialized in {sw.ElapsedMilliseconds}ms");
        });
    }
    
    public static Dictionary<Tile, float> GetSpeeds(DateTime time)
    {
        var index = time.Hour;
        return _speedsMap[index];
    }
}