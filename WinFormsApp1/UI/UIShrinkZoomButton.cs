using GMap.NET.WindowsForms;
using TaxiManager.BasicComponent;

namespace TaxiManager.UI
{
    internal class UIShrinkZoomButton : UIButton
    {
        private Button _shrinkButton;

        public UIShrinkZoomButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _shrinkButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Regular, GraphicsUnit.Point, 134),
                Location = new Point(847, 464),
                Name = "_shrinkButton",
                Size = new Size(40, 40),
                TabIndex = 1,
                Text = "-",
                UseVisualStyleBackColor = true
            };
            _shrinkButton.Click += OnButtonClick;

            _mapForm.Controls.Add(_shrinkButton);
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
        }

        public override void Update(ControlPanel panel)
        {
        }

        private void OnButtonClick(object? sender, EventArgs e)
        {
            _mapForm.GMap.Zoom--;
        }
    }
}
