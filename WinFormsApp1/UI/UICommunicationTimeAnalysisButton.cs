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
    public partial class UICommunicationTimeAnalysisButton : UIButton
    {
        public const string KeyF9Title = "F9Title";
        public const string KeyF9Result = "F9Result";
        public const string KeyF9ScrollTime = "F9ScrollTime";

        private SideBarLabel _resultLabel;
        private Button _communicationTimeAnalysisButton;

        public UICommunicationTimeAnalysisButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm) { }

        public override void Initialize()
        {
            _communicationTimeAnalysisButton = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(760, 360),
                Name = "_communicationTimeAnalysisButton",
                Size = new Size(121, 40),
                TabIndex = 8,
                Text = "通信时间分析",
                UseVisualStyleBackColor = true
            };
            _communicationTimeAnalysisButton.Click += OnButtonClick;

            _mapForm.Controls.Add(_communicationTimeAnalysisButton);
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

        public void ResetCommunicationTimeAnalysisButton()
        {
            SelectRegion.Instance.EndSelectRegion();
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
            registry.Add((KeyF9Title, new SideBarLabel("F9通信时间分析", ContentAlignment.MiddleCenter)));
            
            // 添加时间选择器
            registry.Add((KeyF9ScrollTime, new SideBarScrollTime()));
            
            _resultLabel = new SideBarLabel("请先选择起点区域A和终点区域B，然后选择时间段");
            registry.Add((KeyF9Result, _resultLabel));
        }

        public void StartAnalysis()
        {
            try
            {
                var regions = SelectRegion.Instance.GetRegions();
                if (regions.Count != 2)
                {
                    _resultLabel.SetValue("请选择两个区域（起点A和终点B）");
                    return;
                }

                // 获取用户选择的时间
                var timeObj = _mapForm.ControlPanel.GetItemValue(KeyF9ScrollTime);
                var time = (DateTime?)timeObj;
                if (time == null)
                {
                    _resultLabel.SetValue("请在侧边栏选择时间段");
                    return;
                }

                if (!Speeds.Loaded || !Paths.Loaded)
                {
                    try { _resultLabel.SetValue("数据尚未完成预处理，请等待大约1~5秒"); } catch { }
                    return;
                }

                // UI反馈
                try { _resultLabel.SetValue("F9: 正在计算最短通行时间路径..."); } catch { }

                var sw = Stopwatch.StartNew();
                
                // 调用服务获取最短时间路径
                var result = IServiceF9.Instance.GetShortestPath(regions[0], regions[1], time.Value);

                long calcTime = sw.ElapsedMilliseconds;

                if (result == null)
                {
                    try 
                    { 
                        _resultLabel.SetValue($"F9: 未找到从区域A到区域B的有效路径\n\n可能原因：\n" +
                            $"1. 两个区域之间没有连通的速率数据\n" +
                            $"2. 所选时段({time.Value.Hour}:00-{time.Value.Hour+1}:00)交通流量过低\n" +
                            $"3. 区域位置选择不当\n\n耗时: {sw.ElapsedMilliseconds}ms"); 
                    } catch { }
                    return;
                }

                var (route, distance, pathTime) = result.Value;

                // 在地图上绘制路径
                _mapForm.ControlPanel.ModifyOverlay((overlay) =>
                {
                    // 清除旧的路径
                    var oldRoutes = overlay.Routes.Where(r => r.Name?.StartsWith("F9_") == true).ToList();
                    foreach (var r in oldRoutes)
                        overlay.Routes.Remove(r);

                    // 绘制新路径
                    var routeOverlay = new GMapRoute(route.Points, "F9_ShortestPath")
                    {
                        Stroke = new Pen(Color.Red, 6),
                    };

                    overlay.Routes.Add(routeOverlay);

                    // 标记起点和终点
                    if (route.Points.Count > 0)
                    {
                        var startPoint = route.Points.First();
                        var endPoint = route.Points.Last();

                        var startMarker = new GMarkerDot(startPoint, 5, Color.Red);
                        startMarker.ToolTipText = $"起点区域A\n时间: {pathTime.TotalMinutes:F1}min";
                        
                        var endMarker = new GMarkerDot(endPoint, 5, Color.Red);
                        endMarker.ToolTipText = $"终点区域B\n时间: {pathTime.TotalMinutes:F1}min";

                        overlay.Markers.Add(startMarker);
                        overlay.Markers.Add(endMarker);
                    }
                });

                // 显示结果
                var resultText = $"✓ 找到最短通行时间路径\n\n";
                resultText += $"通行时间段: {time.Value.Hour:D2}:00 - {(time.Value.Hour+1)%24:D2}:00\n";
                resultText += $"路径点数: {route.Points.Count}\n";
                resultText += $"总距离: {distance:F2}公里\n";
                resultText += $"预计通行时间: {pathTime.TotalMinutes:F1}分钟\n";
                resultText += $"\n总耗时: {sw.ElapsedMilliseconds}ms, 计算耗时: {calcTime}ms";

                try { _resultLabel.SetValue(resultText); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"F9 分析失败: {ex.Message}\n{ex.StackTrace}", "错误");
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
