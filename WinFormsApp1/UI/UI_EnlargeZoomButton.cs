using GMap.NET.WindowsForms;
using System.Windows.Forms;

namespace TaxiManager
{
    public class UI_EnlargeZoomButton : UI_Button
    {
        private Button _enlargeButton;

        public UI_EnlargeZoomButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _enlargeButton = new Button();

            _enlargeButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _enlargeButton.BackColor = SystemColors.ButtonHighlight;
            _enlargeButton.FlatAppearance.BorderColor = Color.White;
            _enlargeButton.FlatAppearance.BorderSize = 0;
            _enlargeButton.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Regular, GraphicsUnit.Point, 134);
            _enlargeButton.Location = new Point(847, 418);
            _enlargeButton.Name = "_enlargeButton";
            _enlargeButton.Size = new Size(40, 40);
            _enlargeButton.TabIndex = 0;
            _enlargeButton.Text = "+";
            _enlargeButton.UseVisualStyleBackColor = false;
            _enlargeButton.Click += _enlargeButtonClick;

            _mapForm.Controls.Add(_enlargeButton);
        }

        private void _enlargeButtonClick(object sender, EventArgs e)
        {
            _mapForm.gmap.Zoom++;
        }

        
    }
}
