using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TaxiManager;
using TaxiManager.BasicComponent;
using TaxiManager.Service;
using TaxiManager.Structure;

namespace TaxiManager.UI
{
    public partial class UIRegionCorrelation2AnalyzingButton : UIButton
    {
        public const string KeyF6Title = "F6Title";
        public const string KeyScrollTime = "ScrollTime";
        public const string KeyResult = "RegionCorrelation2AnalyzingResult";

        private SideBarLabel _resultLabel;

        private Button _regionalCorrelationAnalysis2Button;

        private int _timer = 0;
        private int _routineTicks = 1;


        public UIRegionCorrelation2AnalyzingButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _regionalCorrelationAnalysis2Button = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(760, 210),
                Name = "_regionalCorrelationAnalysis2Button",
                Size = new Size(121, 40),
                TabIndex = 5,
                Text = "区域关联分析2",
                UseVisualStyleBackColor = true
            };
            _regionalCorrelationAnalysis2Button.Click += OnButtonClick;

            _mapForm.Controls.Add(_regionalCorrelationAnalysis2Button);
        }

        // 区域关联分析2按钮点击事件
        public void OnButtonClick(object sender, EventArgs e)
        {
            _mapForm.ResetAllButton();
            if (_mapForm.ControlPanel.CurrentComponent != this)
            {
                _mapForm.ControlPanel.SwitchTo(this);
                SelectRegion.Instance.StartSelectRegion(1);
            }
            else
            {
                _mapForm.ControlPanel.SwitchTo(null);
            }
        }


        // 重置区域关联分析2按钮到初始状态（供其他按钮调用）
        public void ResetCorrelationAnalysis2Button()
        {
            SelectRegion.Instance.EndSelectRegion();

        }

        public void StartAnalysis()
        {
            try
            {
                var region = SelectRegion.Instance.GetOneRegion();
                if (region == null)
                {
                    try { _resultLabel.SetValue("请先在图上选取一个区域"); } catch { }
                    return;
                }

                var timeObj = _mapForm.ControlPanel.GetItemValue(KeyScrollTime);
                var time = (DateTime?)timeObj;
                if (time == null)
                {
                    try { _resultLabel.SetValue("请在侧边栏选择时间"); } catch { }
                    return;
                }

                float flow = IServiceF6.Instance.GetFlowTo(region.Value, time.Value);

                _mapForm.ControlPanel.ModifyOverlay((overlay) =>
                {
                    var corners = region.Value.Corners.Select(p => p.ToGmap()).ToList();
                    var poly = new GMapPolygon(corners, "F6_RegionCorrelation2")
                    {
                        Stroke = new Pen(Color.Yellow, 2),
                        Fill = new SolidBrush(Color.FromArgb(40, Color.Yellow))
                    };
                    overlay.Polygons.Add(poly);
                });

                try { _resultLabel.SetValue($"F6: 流量 = {flow:F2}"); } catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"F6 分析失败: {ex.Message}", "错误");
            }
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
            registry.Add((KeyF6Title, new SideBarLabel("F6区域关联分析2", ContentAlignment.MiddleCenter)));
            registry.Add((KeyScrollTime, new SideBarScrollTime()));
            SideBarButton button = new SideBarButton("开始分析");
            //button.Click += StartAnalysis;
            //registry.Add((KeyRegionCorrelation2AnalyzingButton, button));
            //registry.Add((KeyResult, new SideBarLabel("")));
            //throw new NotImplementedException();
            _resultLabel = new SideBarLabel("请选择区域");
            registry.Add((KeyResult, _resultLabel));
        }

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
