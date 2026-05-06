using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    internal class UI_DebugShowGMapButton : UI_Button
    {
        private Button _debugShowGmapButton;

        private bool _isShowGmap = true;

        public UI_DebugShowGMapButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _debugShowGmapButton = new Button();

            _debugShowGmapButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _debugShowGmapButton.Location = new Point(750, 20);
            _debugShowGmapButton.Name = "_regionSreachButton";
            _debugShowGmapButton.Size = new Size(121, 40);
            _debugShowGmapButton.TabIndex = 2;
            _debugShowGmapButton.Text = "Debug-Show Gmap";
            _debugShowGmapButton.Font=new Font("Microsoft YaHei UI", 5);
            _debugShowGmapButton.UseVisualStyleBackColor = true;
            _debugShowGmapButton.Click += _regionSreachButtonClick;

            _mapForm.Controls.Add(_debugShowGmapButton);
        }

        private void _regionSreachButtonClick(object sender, EventArgs e)
        {
            if (_isShowGmap)
            {
                _mapForm.gmap.Hide();
                _isShowGmap = false;
            }
            else
            {
                _mapForm.gmap.Show();
                _isShowGmap = true;
            }
        }
    }
}
