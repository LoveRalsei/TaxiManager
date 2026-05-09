using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaxiManager.Service;
using TaxiManager.Structure;

namespace TaxiManager.BasicComponent
{
    public partial class UIRegionSreachButton: UIButton
    {
        public const string KeyF3TTitile = "F3Title";
        public const string KeyChooseTimePeriod = "ChooseTimePeriod";
        public const string KeyResult="RegionSearchResult";

        private SideBarLabel _resultLabel;

        private ValueTuple<DateTime, DateTime>? _oldTime=null;
        private PositionRange? _oldRegion = null;

        private Button _regionSreachButton;

        public UIRegionSreachButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm) { }

        public override void Initialize()
        {
            _regionSreachButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(720, 464),
                Name = "_regionSreachButton",
                Size = new Size(121, 40),
                TabIndex = 2,
                Text = "区域范围查找",
                UseVisualStyleBackColor = true
            };
            _regionSreachButton.Click += OnButtonClick;

            _mapForm.Controls.Add(_regionSreachButton);
        }

        private void OnButtonClick(object sender, EventArgs e)
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

        public void ResetRegionSearchButton()
        {
            SelectRegion.Instance.EndSelectRegion();
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
            _resultLabel = new SideBarLabel("请在地图上选择一个矩形区域", ContentAlignment.MiddleLeft);

            registry.Add((KeyF3TTitile, new SideBarLabel("F3区域范围查找",ContentAlignment.MiddleCenter)));
            registry.Add((KeyChooseTimePeriod,new SideBarChooseTimePeriod()));
            registry.Add((KeyResult, _resultLabel));
            SideBarButton button = new("确认查找");
            button.Click += StartSearch;
            registry.Add(("按钮", button));
            
            //throw new NotImplementedException();
        }

        public virtual void StartSearch()
        {
            var time = ((ValueTuple<DateTime, DateTime>?)_mapForm.ControlPanel.GetItemValue(KeyChooseTimePeriod))!.Value;
            var region = SelectRegion.Instance.GetOneRegion();
            if (region == null)
                return;
            if (time == _oldTime && region == _oldRegion)
                return;
            _oldTime = time;
            _oldRegion = region;

            DateTime fromTime = time.Item1;
            DateTime toTime = time.Item2;

            Stopwatch stopwatch = Stopwatch.StartNew();
            uint count = IServiceF3.Instance.CountDrivers((PositionRange)region, fromTime, toTime);
            stopwatch.Stop();

            _resultLabel.SetValue($"查询从\n" +
                $"{fromTime:MM:dd:HH:mm}到\n" +
                $"{toTime:MM:dd:HH:mm}\n" +
                $"期间在选定区域内的出租车数量：\n" +
                $"{count}\n" +
                $"耗时：{stopwatch.ElapsedMilliseconds}ms");
        }

        public override void Update(ControlPanel panel)
        {
        }
    }
}
