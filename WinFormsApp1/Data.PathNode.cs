using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public readonly struct PathNode
    {
        public static readonly string DateFormat = new("yyyy-MM-dd HH:mm:ss");
        public readonly DateTime Time;
        public readonly Position Position;

        public PathNode(DateTime date, double x, double y) : this()
        {
            Date = date;
            Position = Position.MakeFromRaw(x, y);
        }

        public bool IsValid() => Position.IsValid();

        public DateTime Date { get; }
    }
}
