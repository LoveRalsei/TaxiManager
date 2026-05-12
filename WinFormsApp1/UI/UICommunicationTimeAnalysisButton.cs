using GMap.NET.WindowsForms;
using TaxiManager.BasicComponent;

namespace TaxiManager.UI
{
    public partial class UICommunicationTimeAnalysisButton : UIButton
    {
        public const string KeyF9Title = "F9Title";

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
            }
        }

        public void ResetCommunicationTimeAnalysisButton()
        {
            SelectRegion.Instance.EndSelectRegion();
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
            registry.Add((KeyF9Title, new SideBarLabel("F9通信时间分析", ContentAlignment.MiddleCenter)));
        }

        public override void Update(ControlPanel panel)
        {
        }

        public void StartAnalysis()
        {
            // TODO: 实现通信时间分析的具体逻辑
        }
    }
}
