using TaxiManager.Structure;

namespace TaxiManager.Service;

public class ServiceF5 : IServiceF5
{
    public static readonly ServiceF5 Instance = new ServiceF5();
    public (float fromAtoB, float fromBtoA) GetFlow(PositionRange rangeA, PositionRange rangeB, DateTime time)
    {
        var tilesA = rangeA.GetTiles();
        var tilesB = rangeB.GetTiles().ToHashSet();
        var flowAtoB = 0f;
        var flowBtoA = 0f;
        var unit = TimeUnit.GetUnit(time);
        foreach (var tileA in tilesA)
        {
            var flows = Flows.GetFlowFrom(tileA, unit);
            foreach (var tileB in tilesB)
            {
                if (flows.TryGetValue(tileB, out var flow))
                    flowAtoB += flow;
            }
        }

        foreach (var tileB in tilesB)
        {
            var flows = Flows.GetFlowFrom(tileB, unit);
            foreach (var tileA in tilesA)
            {
                if (flows.TryGetValue(tileA, out var flow))
                    flowBtoA += flow;
            }
        }

        return (flowAtoB, flowBtoA);
    }
}