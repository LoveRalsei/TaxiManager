using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiManager.BasicComponent;

namespace TaxiManager.UI
{
    internal class UITrafficFlowAnalysisButton : UIButton
    {


        private Button _trafficFlowAnalysisButton;

        public UITrafficFlowAnalysisButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm){}

        public override void Initialize()
        {
            _trafficFlowAnalysisButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Location = new Point(720, 464),
                Name = "_regionSreachButton",
                Size = new Size(121, 40),
                TabIndex = 2,
                Text = "区域范围查找",
                UseVisualStyleBackColor = true
            };
            _trafficFlowAnalysisButton.Click += OnButtonClick;

            _mapForm.Controls.Add(_trafficFlowAnalysisButton);
        }

        private void OnButtonClick(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
            throw new NotImplementedException();
        }

        public override void Update(ControlPanel panel)
        {
            throw new NotImplementedException();
        }


    }
}
