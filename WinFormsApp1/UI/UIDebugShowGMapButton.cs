using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.UI
{
    internal class UIDebugShowGMapButton : UIButton
    {
        private Button _debugShowGmapButton;

        private bool _isShowGmap = true;

        public UIDebugShowGMapButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _debugShowGmapButton = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(750, 20),
                Name = "_regionSreachButton",
                Size = new Size(121, 40),
                TabIndex = 2,
                Text = "Debug-Show Gmap",
                Font = new Font("Microsoft YaHei UI", 5),
                UseVisualStyleBackColor = true
            };
            _debugShowGmapButton.Click += OnButtonClick;

            _mapForm.Controls.Add(_debugShowGmapButton);
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
        }

        public override void Update(ControlPanel panel)
        {
        }

        private void OnButtonClick(object? sender, EventArgs e)
        {
            if (_isShowGmap)
            {
                _mapForm.GMap.Hide();
                _isShowGmap = false;
            }
            else
            {
                _mapForm.GMap.Show();
                _isShowGmap = true;
            }
        }
    }
}
