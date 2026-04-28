using GMap.NET.WindowsForms;

namespace TaxiManager
{
    public class UI_Button
    {
        protected GMapControl _gmap;
        protected MapForm _mapForm;

        public UI_Button(GMapControl gmap, MapForm mapForm)
        {
            _gmap = gmap;
            _mapForm = mapForm;
        }

        public UI_Button(GMapControl gmap)
        {
            _gmap = gmap;
        }

        public virtual void Initialize()
        {
        }
        
    }
}
