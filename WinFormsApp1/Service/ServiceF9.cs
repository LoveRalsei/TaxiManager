using GMap.NET;
using TaxiManager.Structure;

namespace TaxiManager.Service;

public class ServiceF9 : IServiceF9
{
    public static readonly ServiceF9 Instance = new();

    public (MapRoute path, DateTime time)? GetShortestPath(PositionRange rangeA, PositionRange rangeB, DateTime timeFrom, DateTime timeTo)
    {
        throw new NotImplementedException();
    }
}
