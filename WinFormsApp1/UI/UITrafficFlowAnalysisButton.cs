using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiManager.BasicComponent;
using TaxiManager.Service;
using TaxiManager.Structure;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace TaxiManager.UI
{
    internal class UITrafficFlowAnalysisButton : UIButton
    {
        public const string KeyF4Title = "F4Title";
        public const string KeyChooseTime = "ChooseTime";
        public const string KeyTrafficFlowAnalysisButton = "TrafficFlowAnalysisButton";
        public const string KeyTrafficFlowAnalysisResult = "TrafficFlowAnalysisResult";

        private SideBarLabel _resultLabel;

        private Button _trafficFlowAnalysisButton;

        public UITrafficFlowAnalysisButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm) { }

        public override void Initialize()
        {
            _trafficFlowAnalysisButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(593, 464),
                Name = "_trafficFlowAnalysisButton",
                Size = new Size(121, 40),
                TabIndex = 3,
                Text = "车流密度分析",
                UseVisualStyleBackColor = true
            };
            _trafficFlowAnalysisButton.Click += OnButtonClick;

            _mapForm.Controls.Add(_trafficFlowAnalysisButton);
        }

        private void OnButtonClick(object? sender, EventArgs e)
        {
            _mapForm.ResetAllButton();
            if (_mapForm.ControlPanel.CurrentComponent != this)
            {
                _mapForm.ControlPanel.SwitchTo(this);
            }
            else
            {
                _mapForm.ControlPanel.SwitchTo(null);
            }
        }

        public void ResetTrafficFlowAnalysisButton()
        {
            // no-op for now
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
            registry.Add((KeyF4Title, new SideBarLabel("F4区域车流密度分析", ContentAlignment.MiddleCenter)));
            registry.Add((KeyChooseTime, new SideBarScrollTime()));
            SideBarButton button = new("确认查找");
            button.Click += StartAnalysis;
            registry.Add((KeyTrafficFlowAnalysisButton, button));
            _resultLabel = new SideBarLabel("");
            registry.Add((KeyTrafficFlowAnalysisResult, _resultLabel));
        }

        public async void StartAnalysis()
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                var timeObj = _mapForm.ControlPanel.GetItemValue(KeyChooseTime);
                DateTime? time = (DateTime?)timeObj;
                if (time == null)
                {
                    MessageBox.Show("请在侧边栏选择时间", "提示");
                    return;
                }

                var viewArea = _mapForm.GMap.ViewArea;
                var gmapSize = _mapForm.GMap.Size;

                // guard against zero size
                if (gmapSize.Width == 0 || gmapSize.Height == 0)
                {
                    MessageBox.Show("地图尺寸无效，无法分析", "错误");
                    return;
                }

                // UI feedback
                try { _resultLabel.SetValue("F4: 正在计算..."); } catch { }

                // call service in background
                var tilesMap = await Task.Run(() => ((IServiceF4)ServiceF4.Instance).GetDensityChange(viewArea, gmapSize, time.Value));

                // render on overlay (ControlPanel.ModifyOverlay runs on UI thread)
                _mapForm.ControlPanel.ModifyOverlay((overlay) =>
                {
                    // remove previous F4 visuals
                    var oldPolys = overlay.Polygons.Where(p => p.Name != null && p.Name.StartsWith("F4Tile_")).ToList();
                    foreach (var p in oldPolys) overlay.Polygons.Remove(p);

                    // add new polygons
                    foreach (var kv in tilesMap)
                    {
                        var tile = kv.Key;
                        var color = kv.Value;
                        var range = tile.Range;
                        var minPoint = range.Min.ToGmap();
                        var maxPoint = range.Max.ToGmap();
                        var pts = new List<PointLatLng>
                        {
                            minPoint,
                            new PointLatLng(minPoint.Lat, maxPoint.Lng),
                            maxPoint,
                            new PointLatLng(maxPoint.Lat, minPoint.Lng)
                        };

                        var poly = new GMap.NET.WindowsForms.GMapPolygon(pts, $"F4Tile_{tile.Size}_{tile.X}_{tile.Y}")
                        {
                            Fill = new SolidBrush(color),
                            Stroke = new Pen(Color.Transparent, 0)
                        };

                        overlay.Polygons.Add(poly);
                    }
                });

                stopwatch.Stop();
                try { _resultLabel.SetValue($"F4: 渲染完毕，共 {tilesMap.Count} 瓦片\n" +
                        $"耗时{stopwatch.ElapsedMilliseconds}ms"); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"F4 分析失败: {ex.Message}", "错误");
            }
        }

        public override void Update(ControlPanel panel)
        {

        }


    }
}
