using TaxiManager.Structure;

namespace TaxiManager.Service;

public class ServiceF5 : IServiceF5
{
    public static readonly ServiceF5 Instance = new ServiceF5();
    public (int fromAtoB, int fromBtoA) GetFlow(PositionRange rangeA, PositionRange rangeB, DateTime timeFrom, DateTime timeTo)
    {
        var flowAtoB = 0f;
        var flowBtoA = 0f;
        var unitFrom = TimeUnit.GetUnit(timeFrom);
        var unitTo = TimeUnit.GetUnit(timeTo);
        
        var flow = Flows.GetFlowPeriod(rangeA, rangeB, unitFrom, unitTo);

        return (flow.fromAtoB, flow.fromBtoA);
    }
}