using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TaxiManager.BasicComponent;
using TaxiManager.Service;
using TaxiManager.Structure;

namespace TaxiManager.UI
{
    public partial class UIFrequentPathAnalysis2Button : UIButton
    {
        public const string KeyF8Title = "F8Title";
        public const string KeyF8Result = "F8Result";
        public const string KeyF8PathCountInput = "F8PathCountInput";

        private SideBarLabel _resultLabel;
        private Button _frequentPathAnalysis2Button;

        public UIFrequentPathAnalysis2Button(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm) { }

        public override void Initialize()
        {
            _frequentPathAnalysis2Button = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(760, 310),
                Name = "_frequentPathAnalysis2Button",
                Size = new Size(121, 40),
                TabIndex = 7,
                Text = "频繁路径分析2",
                UseVisualStyleBackColor = true
            };
            _frequentPathAnalysis2Button.Click += OnButtonClick;

            _mapForm.Controls.Add(_frequentPathAnalysis2Button);
        }

        private void OnButtonClick(object? sender, EventArgs e)
        {
            _mapForm.ResetAllButton();
            if (_mapForm.ControlPanel.CurrentComponent != this)
            {
                _mapForm.ControlPanel.SwitchTo(this);
                SelectRegion.Instance.StartSelectRegion(2);
            }
            else
            {
                _mapForm.ControlPanel.SwitchTo(null);
                SelectRegion.Instance.EndSelectRegion();
            }
        }

        public void ResetFrequentPathAnalysis2Button()
        {
            SelectRegion.Instance.EndSelectRegion();
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
            registry.Add((KeyF8Title, new SideBarLabel("F8频繁路径分析2", ContentAlignment.MiddleCenter)));
            registry.Add((KeyF8PathCountInput, new SideBarInput("请输入要显示的路径数量 (K值)", "5")));
            
            SideBarButton analyzeButton = new("开始分析");
            analyzeButton.Click += StartAnalysis;
            registry.Add(("F8AnalyzeButton", analyzeButton));
            
            _resultLabel = new SideBarLabel("请先选择起点区域A和终点区域B");
            registry.Add((KeyF8Result, _resultLabel));
        }
        

        public void StartAnalysis()
        {
            try
            {
                var regions = SelectRegion.Instance.GetRegions();
                if (regions.Count != 2)
                    return;
                
                // 确保Paths已初始化
                if (!Paths.Loaded)
                {
                    MessageBox.Show("路径数据正在加载中，请稍等...");
                    return;
                }

                // 获取用户输入的K值
                var kInput = (string)_mapForm.ControlPanel.GetItemValue(KeyF8PathCountInput)!;
                if (!int.TryParse(kInput, out int k) || k <= 0)
                {
                    MessageBox.Show("请输入有效的路径数量 (正整数)", "输入错误");
                    return;
                }
                
                // UI反馈
                try { _resultLabel.SetValue("F8: 正在分析频繁路径..."); } catch { }

                var sw = Stopwatch.StartNew();
                // 调用服务获取前K个OD频繁路径
                var odPaths = IServiceF8.Instance.GetTopKFrequentPaths(regions[0], regions[1], k);

                // 在地图上绘制路径
                _mapForm.ControlPanel.ModifyOverlay((overlay) =>
                {
                    // 绘制路径
                    for (int i = 0; i < odPaths.Count; i++)
                    {
                        var path = odPaths[i];
                        
                        // 为每条路径生成不同颜色
                        var hue = (i * 137.508) % 360; // 黄金角度分布
                        var color = HslToRgb(hue / 360.0, 0.7, 0.5);

                        // 创建路径点列表
                        var points = new List<PointLatLng>();
                        foreach (var tile in path.PathTiles)
                        {
                            var center = GetTileCenter(tile);
                            points.Add(center.ToGmap());
                        }

                        // 如果路径点太少，跳过
                        if (points.Count < 2) continue;

                        // 创建路径线
                        var route = new GMapRoute(points, $"ODPath_{i}")
                        {
                            Stroke = new Pen(color, 6),
                        };

                        overlay.Routes.Add(route);
                    }
                });
                sw.Stop();

                // 显示结果
                var resultText = $"找到 {odPaths.Count} 条从A到B的频繁路径\n\n";
                
                if (odPaths.Any())
                {
                    for (int i = 0; i < odPaths.Count; i++)
                    {
                        var path = odPaths[i];
                        resultText += $"路径{i+1}: 频率={path.Frequency:F2}, 长度={path.LengthMeters:F0}米 ({path.LengthMeters/1000:F2}公里)\n";
                    }
                    resultText += "\n耗时: " + sw.ElapsedMilliseconds + "ms";
                }
                else
                {
                    resultText += "未找到符合条件的路径，请尝试:\n";
                    resultText += "1. 调整区域A和B的位置\n";
                    resultText += "2. 增大区域范围\n";
                    resultText += "3. 选择交通更繁忙的区域";
                }

                try { _resultLabel.SetValue(resultText); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"F8 分析失败: {ex.Message}\n{ex.StackTrace}", "错误");
            }
        }
        
        /// <summary>
        /// HSL转RGB颜色
        /// </summary>
        private Color HslToRgb(double h, double s, double l)
        {
            double r, g, b;
            
            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                var hue2rgb = (double p, double q, double t) =>
                {
                    if (t < 0) t += 1;
                    if (t > 1) t -= 1;
                    if (t < 1.0 / 6) return p + (q - p) * 6 * t;
                    if (t < 1.0 / 2) return q;
                    if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
                    return p;
                };
                
                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                var p = 2 * l - q;
                r = hue2rgb(p, q, h + 1.0 / 3);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - 1.0 / 3);
            }
            
            return Color.FromArgb(
                (int)Math.Round(r * 255),
                (int)Math.Round(g * 255),
                (int)Math.Round(b * 255)
            );
        }

        /// <summary>
        /// 获取瓦片中心点坐标
        /// </summary>
        private Position GetTileCenter(Tile tile)
        {
            var range = tile.Range;
            var centerX = (range.Min.X + range.Max.X) / 2;
            var centerY = (range.Min.Y + range.Max.Y) / 2;
            return Position.From(centerX, centerY);
        }

        public override void Update(ControlPanel panel)
        {
            // 不自动更新，由用户手动触发
        }
    }
}
