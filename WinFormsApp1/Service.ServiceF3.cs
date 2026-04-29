using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public class ServiceF3 : IServiceF3
    {
        public static readonly ServiceF3 Instance = new();
        uint IServiceF3.CountDrivers(PositionRange range, DateTime from, DateTime to)
        {
            foreach (var driver in DataLoader.Drivers)
            {
                
            }
            throw new NotImplementedException();
        }
    }
}
