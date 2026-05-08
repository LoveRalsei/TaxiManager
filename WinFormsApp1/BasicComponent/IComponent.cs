using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiManager.BasicComponent;

namespace TaxiManager.BasicComponent
{
    public interface IComponent
    {
        void RegisterBars(List<(string key, SideBarItem item)> registry);
        void Update(ControlPanel panel);
    }
}
