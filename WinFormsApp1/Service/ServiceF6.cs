using TaxiManager.Structure;

namespace TaxiManager.Service;

public class ServiceF6 : IServiceF6
{
    public static readonly ServiceF6 Instance = new();
    
    public float GetFlowTo(PositionRange toRange, DateTime time)
    {
        var tiles = toRange.GetTiles();
        var unit = TimeUnit.GetUnit(time);
        var flowCount = tiles.Select(tile => Flows.GetFlowTo(tile, unit))
            .Select(flows => flows.Values.Sum())
            .Sum();
        return flowCount;
    }
}