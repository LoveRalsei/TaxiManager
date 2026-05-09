using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiManager.BasicComponent;

namespace TaxiManager.BasicComponent
{
    //侧边栏文本组件
    internal class SideBarLabel : SideBarItem
    {
        private Label _label = new Label
        {
            AutoSize = true
        };
        public SideBarLabel(string text, ContentAlignment textAlign = ContentAlignment.MiddleLeft) : base(null)
        {
            _label.Text = text;
            _label.TextAlign = textAlign;
            AddControlComponent(_label);
        }
        public override object? GetValue()
        {
            return Text;
        }
        public override void SetValue(object? value)
        {
            if (value == null)
                _label.Text = "";
            else 
                _label.Text = value!.ToString() ?? "";
            UpdateSize();
        }
    }
}
