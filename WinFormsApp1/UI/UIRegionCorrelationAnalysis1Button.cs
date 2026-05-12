using GMap.NET;
using GMap.NET.WindowsForms;
using System.Diagnostics;
using TaxiManager.BasicComponent;
using TaxiManager.Service;

namespace TaxiManager.UI
{
    public partial class UIRegionCorrelation1AnalyzingButton : UIButton
    {
        public const string KeyF5Title = "F5Title";
        public const string KeyChooseTimePeriod = "ChooseTimePeriod";
        public const string KeyRegionCorrelation1AnalyzingButton = "RegionCorrelation1AnalyzingButton";
        public const string KeyResult= "RegionCorrelation1AnalyzingResult";

        private SideBarLabel _resultLabel;

        private Button _regionalCorrelationAnalysis1Button;

        
        public UIRegionCorrelation1AnalyzingButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _regionalCorrelationAnalysis1Button = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(760, 160),
                Name = "_regionalCorrelationAnalysis1Button",
                Size = new Size(121, 40),
                TabIndex = 4,
                Text = "区域关联分析1",
                UseVisualStyleBackColor = true
            };
            _regionalCorrelationAnalysis1Button.Click += OnButtonClick;

            _mapForm.Controls.Add(_regionalCorrelationAnalysis1Button);
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
            }
        }


        public void ResetCorrelationAnalysis1Button()
        {
            SelectRegion.Instance.EndSelectRegion();

        }



        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
            registry.Add((KeyF5Title, new SideBarLabel("F5区域关联分析1", ContentAlignment.MiddleCenter)));
            registry.Add((KeyChooseTimePeriod, new SideBarChooseTimePeriod()));
            SideBarButton button = new SideBarButton("开始分析");
            button.Click += StartAnalysis;
            registry.Add((KeyRegionCorrelation1AnalyzingButton, button));
            _resultLabel = new SideBarLabel("");
            registry.Add((KeyResult, _resultLabel));//_resultLabel.SetValue();
        }

        public void StartAnalysis()
        {
            var regions = SelectRegion.Instance.GetTileRegions();
            if (regions.Count != 2)
                return;

            var time = ((ValueTuple<DateTime, DateTime>?)_mapForm.ControlPanel.GetItemValue(KeyChooseTimePeriod))!.Value;
            
            
            //MessageBox.Show("分析函数未完成");
            Stopwatch stopwatch = Stopwatch.StartNew();
            var (fromAtoB, fromBtoA) = IServiceF5.Instance.GetFlow(regions[0], regions[1], time.Item1, time.Item2);
            stopwatch.Stop();

            _resultLabel.SetValue($"区域关联分析结果：\n" +
                $"A→B 流量: {fromAtoB}\n" +
                $"B→A 流量: {fromBtoA}\n" +
                $"耗时：{stopwatch.ElapsedMilliseconds}ms");
        }

        public override void Update(ControlPanel panel)
        {
            //throw new NotImplementedException();
        }
    }
}
