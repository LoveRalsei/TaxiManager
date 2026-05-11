using TaxiManager.Structure;

namespace TaxiManager.Service;

public interface IServiceF5
{
    public static IServiceF5 Instance => ServiceF5.Instance;
    public (int fromAtoB, int fromBtoA) GetFlow(PositionRange rangeA, PositionRange rangeB, DateTime time);
}