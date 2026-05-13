using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TaxiManager.BasicComponent;
using TaxiManager.Service;
using TaxiManager.Structure;

namespace TaxiManager.UI
{
    public partial class UIFrequentPathAnalysis1Button : UIButton
    {
        public const string KeyF7Title = "F7Title";
        public const string KeyFrequentPathAnalysisResult = "FrequentPathAnalysisResult";
        public const string KeyFrequentPathCountInput = "FrequentPathCountInput";
        public const string KeyFrequentPathMinDistanceInput = "KeyFrequentPathMinDistanceInput";

        private SideBarLabel _resultLabel;
        private Button _frequentPathAnalysis1Button;

        public UIFrequentPathAnalysis1Button(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm) { }

        public override void Initialize()
        {
            _frequentPathAnalysis1Button = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(760, 260),
                Name = "_frequentPathAnalysis1Button",
                Size = new Size(121, 40),
                TabIndex = 6,
                Text = "频繁路径分析1",
                UseVisualStyleBackColor = true
            };
            _frequentPathAnalysis1Button.Click += OnButtonClick;

            _mapForm.Controls.Add(_frequentPathAnalysis1Button);
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

        public void ResetFrequentPathAnalysis1Button()
        {
            // no-op for now
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
            registry.Add((KeyF7Title, new SideBarLabel("F7频繁路径分析1", ContentAlignment.MiddleCenter)));
            registry.Add((KeyFrequentPathCountInput, new SideBarInput("请输入要显示的路径数量 (K值)", "10")));
            registry.Add((KeyFrequentPathMinDistanceInput, new SideBarInput("请输入最小距离 (单位: 米)", "100")));
            
            SideBarButton button = new("开始分析");
            button.Click += StartAnalysis;
            registry.Add(("FrequentPathAnalysisButton", button));
            
            _resultLabel = new SideBarLabel("");
            registry.Add((KeyFrequentPathAnalysisResult, _resultLabel));
        }

        public void StartAnalysis()
        {
            try
            {
                // 确保Paths已初始化
                if (!Paths.Loaded)
                {
                    MessageBox.Show("路径数据正在加载中，请稍等...");
                    return;
                }

                // 获取用户输入的K值
                var kInput = (string)_mapForm.ControlPanel.GetItemValue(KeyFrequentPathCountInput)!;
                if (!int.TryParse(kInput, out int k) || k <= 0)
                {
                    MessageBox.Show("请输入有效的路径数量 (正整数)", "输入错误");
                    return;
                }
                var minDistanceInput = (string)_mapForm.ControlPanel.GetItemValue(KeyFrequentPathMinDistanceInput)!;
                if (!double.TryParse(minDistanceInput, out double minDistance))
                {
                    MessageBox.Show("请输入有效的最小距离 (单位: 米)", "输入错误");
                    return;
                }
                
                // UI反馈
                try { _resultLabel.SetValue("F7: 正在分析频繁路径..."); } catch { }

                // 调用服务获取前K个频繁路径
                var frequentPaths = IServiceF7.Instance.GetTopKFrequentPaths(k, minDistance);

                // 在地图上绘制路径
                _mapForm.ControlPanel.ModifyOverlay((overlay) =>
                {
                    var random = new Random();
                    for (int i = 0; i < frequentPaths.Count; i++)
                    {
                        var path = frequentPaths[i];
                        
                        // 为每条路径生成随机颜色
                        var color = Color.FromArgb(
                            255, 
                            random.Next(50, 200),
                            random.Next(50, 200),
                            random.Next(50, 200)
                        );

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
                        var route = new GMapRoute(points, $"FrequentPath_{i}")
                        {
                            Stroke = new Pen(color, 5),
                        };

                        overlay.Routes.Add(route);
                    }
                });

                // 显示结果
                var resultText = $"F7: 找到 {frequentPaths.Count} 条频繁路径\n";
                resultText += $"显示前 {Math.Min(k, frequentPaths.Count)} 条路径\n\n";
                
                if (frequentPaths.Any())
                {
                    // 显示前5条路径的详细信息
                    var displayCount = Math.Min(5, frequentPaths.Count);
                    for (int i = 0; i < displayCount; i++)
                    {
                        var path = frequentPaths[i];
                        resultText += $"路径{i+1}: 频率={path.Frequency:F2}, 长度={path.LengthMeters:F0}米 ({path.LengthMeters/1000:F2}公里), 瓦片数={path.PathTiles.Count}\n";
                    }
                    
                    if (frequentPaths.Count > 5)
                    {
                        resultText += $"... 还有 {frequentPaths.Count - 5} 条路径";
                    }
                }

                try { _resultLabel.SetValue(resultText); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"F7 分析失败: {ex.Message}\n{ex.StackTrace}", "错误");
            }
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
