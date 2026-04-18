using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public partial class MapForm : Form
    {
        private int _oringinMinZoom = 3;
        private int _oringinMaxZoom = 18;
        private bool _lockZoom = false;
        private void _enlargeButtonClick(object sender, EventArgs e)
        {
            if (_lockZoom) return;
            gmap.Zoom++;
        }

        private void _shrinkButtonClick(object sender, EventArgs e)
        {
            if (_lockZoom) return;
            gmap.Zoom--;
        }
    }
}
