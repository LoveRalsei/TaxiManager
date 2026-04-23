using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public readonly struct PathNode(DateTime time, Position position)
    {
        public static readonly string DateFormat = new("yyyy-MM-dd HH:mm:ss");
        public readonly DateTime Time = time;
        public readonly Position Position = position;

        public bool IsValid() => Position.IsValid();
    }
}
