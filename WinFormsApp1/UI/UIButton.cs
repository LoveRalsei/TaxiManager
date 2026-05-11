using GMap.NET.WindowsForms;
using TaxiManager.BasicComponent;

namespace TaxiManager.UI
{
    public abstract class UIButton : IComponent
    {
        protected GMapControl _gmap;
        protected MapForm _mapForm;

        public UIButton(GMapControl gmap, MapForm mapForm)
        {
            _gmap = gmap;
            _mapForm = mapForm;
        }

        public virtual void Initialize()
        {
        }

        public abstract void RegisterBars(List<(string key, SideBarItem item)> registry);
        public abstract void Update(ControlPanel panel);
    }
}
