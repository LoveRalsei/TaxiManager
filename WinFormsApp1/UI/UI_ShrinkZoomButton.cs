using GMap.NET.WindowsForms;

namespace TaxiManager
{
    internal class UI_ShrinkZoomButton : UI_Button
    {
        private Button _shrinkButton;

        public UI_ShrinkZoomButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _shrinkButton = new Button();

            _shrinkButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _shrinkButton.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Regular, GraphicsUnit.Point, 134);
            _shrinkButton.Location = new Point(847, 464);
            _shrinkButton.Name = "_shrinkButton";
            _shrinkButton.Size = new Size(40, 40);
            _shrinkButton.TabIndex = 1;
            _shrinkButton.Text = "-";
            _shrinkButton.UseVisualStyleBackColor = true;
            _shrinkButton.Click += _shrinkButtonClick;

            _mapForm.Controls.Add(_shrinkButton);
        }

        private void _shrinkButtonClick(object sender, EventArgs e)
        {
            _gmap.Zoom--;
        }
    }
}
