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
        public const string KeyScrollTime = "ScrollTime";
        public const string KeyTrafficFlowAnalysisButton = "TrafficFlowAnalysisButton";
        public const string KeyTrafficFlowAnalysisResult = "TrafficFlowAnalysisResult";
        public const string KeyTrafficFlowAnalysisInput = "TrafficFlowAnalysisInput";

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
            registry.Add((KeyScrollTime, new SideBarScrollTime()));
            /*
            SideBarButton button = new("确认查找");
            button.Click += StartAnalysis;
            registry.Add((KeyTrafficFlowAnalysisButton, button));
            */
            registry.Add((KeyTrafficFlowAnalysisInput, new SideBarInput("请输入网格大小 (单位: 百米)", "1")));
            _resultLabel = new SideBarLabel("");
            registry.Add((KeyTrafficFlowAnalysisResult, _resultLabel));
        }
        
        private string _lastInput = string.Empty;
        private byte _tileSize = 0;

        public void StartAnalysis()
        {
            try
            {
                if (!TileDensity.Loaded)
                    return;
                
                var totalWatch = Stopwatch.StartNew();
                
                var timeObj = _mapForm.ControlPanel.GetItemValue(KeyScrollTime);
                var time = (DateTime?)timeObj;
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

                var currInput = (string)_mapForm.ControlPanel.GetItemValue(KeyTrafficFlowAnalysisInput)!;
                if (!_lastInput.Equals(currInput))
                {
                    _tileSize = !byte.TryParse(currInput, out var result) ? (byte)0 : result;
                    if (_tileSize == 0)
                        MessageBox.Show("请输入正确的网格大小 (1~255)", "确认错误");
                }
                _lastInput = currInput;
                if (_tileSize == 0)
                    return;

                // UI feedback
                try { _resultLabel.SetValue("F4: 正在计算..."); } catch { }

                var calMsWatch = Stopwatch.StartNew();
                var tilesMap = IServiceF4.Instance.GetDensityChange(_tileSize, viewArea, time.Value);
                long calMs = 0;
                calMsWatch.Stop();
                calMs = calMsWatch.ElapsedMilliseconds;
                
                // render on overlay (ControlPanel.ModifyOverlay runs on UI thread)
                _mapForm.ControlPanel.ModifyOverlay((overlay) =>
                {
                    var brushCache = new Dictionary<Color, SolidBrush>();
                    // add new polygons
                    foreach (var kv in tilesMap)
                    {
                        var tile = kv.Key;
                        var color = kv.Value;

                        if (!brushCache.TryGetValue(color, out SolidBrush brush))
                        {
                            brush = new SolidBrush(color);
                            brushCache.Add(color, brush);
                        }
                        
                        var range = tile.Range;
                        var minPoint = range.Min.ToGmap();
                        var maxPoint = range.Max.ToGmap();

                        var poly = new GMapPolygon([
                                minPoint,
                                new PointLatLng(minPoint.Lat, maxPoint.Lng),
                                maxPoint,
                                new PointLatLng(maxPoint.Lat, minPoint.Lng)]
                            , $"F4Tile_{tile.Size}_{tile.X}_{tile.Y}")
                        {
                            Fill = brush,
                            Stroke = Pens.Transparent
                        };

                        overlay.Polygons.Add(poly);
                    }
                });

                totalWatch.Stop();
                try { _resultLabel.SetValue($"F4: 渲染完毕，共 {tilesMap.Count} 瓦片\n" +
                        $"共耗时{totalWatch.ElapsedMilliseconds}ms\n" + 
                        $"其中计算耗时{calMs}ms"); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"F4 分析失败: {ex.Message}", "错误");
            }
        }

        private int _timer = 0;
        private int _routineTicks = 1;

        public override void Update(ControlPanel panel)
        {
            if (_timer++ >= _routineTicks)
            {
                _timer = 0;
                return;
            }
            StartAnalysis();
        }

    }
}
