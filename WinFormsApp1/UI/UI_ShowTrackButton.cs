using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.UI
{
    internal class UI_ShowTrackButton : UI_Button
    {
        private Button _showTrackButton;

        public UI_ShowTrackButton(GMapControl gmap, MapForm mapForm) : base(gmap, mapForm)
        {
        }

        public override void Initialize()
        {
            _showTrackButton = new Button();

            _showTrackButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _showTrackButton.Location = new Point(847, 370);
            _showTrackButton.Name = "_showTrackButton";
            _showTrackButton.Size = new Size(40, 40);
            _showTrackButton.TabIndex = 7;
            _showTrackButton.Text = "轨";
            _showTrackButton.UseVisualStyleBackColor = true;
            _showTrackButton.Click += _regionSreachButtonClick;

            _mapForm.Controls.Add(_showTrackButton);
        }

        private void _regionSreachButtonClick(object sender, EventArgs e)
        {

        }
    }
}
