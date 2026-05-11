using TaxiManager.Structure;

namespace TaxiManager.Service;

public interface IServiceF6
{
    public static IServiceF6 Instance => ServiceF6.Instance;
    public float GetFlowTo(PositionRange toRange, DateTime time);
}