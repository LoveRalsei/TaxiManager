using GMap.NET.WindowsForms;
using TaxiManager.BasicComponent;

namespace TaxiManager.UI
{
    public partial class UIFrequentPathAnalysis1Button : UIButton
    {
        public const string KeyF7Title = "F7Title";

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
        }

        public override void Update(ControlPanel panel)
        {
        }

        public void StartAnalysis()
        {
            // TODO: 实现频繁路径分析1的具体逻辑
        }
    }
}
