using GMap.NET;
using TaxiManager.Structure;

namespace TaxiManager.Service;

public interface IServiceF9
{
    public static IServiceF9 Instance => ServiceF9.Instance;

    public (MapRoute path, double distance, TimeSpan time)? GetShortestPath(PositionRange rangeA, PositionRange rangeB, DateTime time);
}
