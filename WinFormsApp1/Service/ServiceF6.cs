using TaxiManager.Structure;

namespace TaxiManager.Service;

public class ServiceF6 : IServiceF6
{
    public static readonly ServiceF6 Instance = new();
    
    public float GetFlowTo(PositionRange toRange, DateTime time)
    {
        var unit = TimeUnit.GetUnit(time);
        var flows = Flows.GetFlowTo(toRange, unit);
        return flows.Values.Sum();
    }
}