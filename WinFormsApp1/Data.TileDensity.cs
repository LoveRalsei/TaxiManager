using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public class TileDensity
    {
        private readonly static Dictionary<(Tile tile, DateTime time), uint> _densityMap = [];
    }
}
