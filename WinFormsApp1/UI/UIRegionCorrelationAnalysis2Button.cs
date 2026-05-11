using GMap.NET;
using GMap.NET.WindowsForms;
using TaxiManager.BasicComponent;

namespace TaxiManager.UI
{
    public class UI_RegionCorrelation2AnalyzingButton : UIButton
    {
        public const string KeyF6Title = "F6Title";
        public const string KeyScrollTime = "ScrollTime";
        public const string KeyResult = "RegionCorrelation2AnalyzingResult";

        private SideBarLabel _resultLabel;

        private Button _regionalCorrelationAnalysis2Button;


        public UI_RegionCorrelation2AnalyzingButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _regionalCorrelationAnalysis2Button = new Button();

            _regionalCorrelationAnalysis2Button = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(339, 464),
                Name = "_regionalCorrelationAnalysis2Button",
                Size = new Size(121, 40),
                TabIndex = 5,
                Text = "区域关联分析2",
                UseVisualStyleBackColor = true
            };
            _regionalCorrelationAnalysis2Button.Click += _regionalCorrelationAnalysis2ButtonClick;

            _mapForm.Controls.Add(_regionalCorrelationAnalysis2Button);
        }

        // 区域关联分析2按钮点击事件
        private void _regionalCorrelationAnalysis2ButtonClick(object sender, EventArgs e)
        {
            _mapForm.ResetAllButton();
            if (_mapForm.ControlPanel.CurrentComponent != this)
            {
                _mapForm.ControlPanel.SwitchTo(this);
                SelectRegion.Instance.StartSelectTileRegion(1, 1);
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
            // TODO: 在这里实现区域关联分析2的具体逻辑（分析一个区域与其他区域的关联）
            // region 是一个区域的四个角点经纬度
            // startTime / endTime 为 "yyyy-MM-dd" 格式的时间范围
            // 示例：region[0].Lat / .Lng 可直接使用
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
            //throw new NotImplementedException();
        }
    }
}
