using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.BasicComponent
{
    internal class SideBarButton : SideBarItem
    {
        private Button? _button;
        private FlowLayoutPanel? _buttonLayout;
        public new event Action? Click;
        public SideBarButton(string text) : base(text)
        {
        }

        public override void InitComponents()
        {
            _button = new Button
            {
                Text = _text,
                AutoSize = true,
                MinimumSize = new Size(50, 30),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter
            };
            //MessageBox.Show(_text);
            _button.Click += (_,_) => Click?.Invoke();
            _buttonLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };
            _buttonLayout.Controls.Add(_button);
            AddControlComponent(_buttonLayout);
        }

        public override object? GetValue()
        {
            throw new NotImplementedException();
        }
    }
}
