using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public interface IServiceF3
    {
        public static IServiceF3 Instance => throw new NotImplementedException();
        public abstract uint CountDrivers(PositionRange range, DateTime from, DateTime to);
    }
}
