using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            /*Stopwatch stopwatch = Stopwatch.StartNew();
            _mapForm.gmap.HoldInvalidation = true;
            var testOverlay = new GMapOverlay("test");
            for (int i = 0; i < 500; i++)
            {
                var points = new List<PointLatLng>
{
    new PointLatLng(39.9 + i*0.001, 116.3),
    new PointLatLng(39.9 + i*0.001, 116.4),
    new PointLatLng(39.8 + i*0.001, 116.4),
    new PointLatLng(39.8 + i*0.001, 116.3)
};
                var rect = new GMapPolygon(points, "rect" + i);
                rect.Fill = new SolidBrush(Color.FromArgb(128, Color.Red));
                testOverlay.Polygons.Add(rect);
            }
            _mapForm.gmap.Overlays.Add(testOverlay);
            _mapForm.gmap.HoldInvalidation = false;
            _mapForm.gmap.Refresh();
            stopwatch.Stop();
            MessageBox.Show($"添加500个矩形耗时: {stopwatch.ElapsedMilliseconds}ms");*/
        }
    }
}
