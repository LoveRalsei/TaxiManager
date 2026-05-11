using GMap.NET.WindowsForms;
using TaxiManager.BasicComponent;

namespace TaxiManager.UI
{
    public class UIEnlargeZoomButton : UIButton
    {
        private Button _enlargeButton;

        public UIEnlargeZoomButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _enlargeButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = SystemColors.ButtonHighlight,
                Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Regular, GraphicsUnit.Point, 134),
                Location = new Point(847, 418),
                Name = "_enlargeButton",
                Size = new Size(40, 40),
                TabIndex = 0,
                Text = "+",
                UseVisualStyleBackColor = false
            };
            _enlargeButton.FlatAppearance.BorderColor = Color.White;
            _enlargeButton.FlatAppearance.BorderSize = 0;
            _enlargeButton.Click += OnButtonClick;

            _mapForm.Controls.Add(_enlargeButton);
        }

        public override void RegisterBars(List<(string key, SideBarItem item)> registry)
        {
            
        }

        public override void Update(ControlPanel panel)
        {
            
        }

        private void OnButtonClick(object? sender, EventArgs e)
        {
            _mapForm.GMap.Zoom++;
        }
        
    }
}
