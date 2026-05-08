using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TaxiManager.UI
{
    public class SideBarChooseCar : SideBarItem
    {
        private TextBox _inputBox;
        private int? _result = null;

        public SideBarChooseCar() : base("点击在输入框内输入汽车ID\n非正数或无效值代表无选择或选择全体") { }

        public override void InitComponents()
        {
            base.InitComponents();

            // 创建输入框
            _inputBox = new TextBox
            {
                Width = 150,
                Height = 28,
                //Margin = new Padding(0, 8, 0, 8),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                BorderStyle = BorderStyle.FixedSingle
            };
            _inputBox.TextChanged += OnInputBoxTextChanged;

            AddControlComponent( _inputBox );

            
        }

        private void OnInputBoxTextChanged(object? sender, EventArgs e)
        {
            string text = _inputBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                _result = null;
                return;
            }

            // 尝试解析输入框中的整数
            if (!int.TryParse(text, out int num))
            {
                _result = null;
                return;
            }

            // 验证条件：超出ID范围
            if (num <= 0 || num > DataLoader.DriversCount)
            {
                _result = null;
                return;
            }

            // 存储结果
            _result = num;
        }

        public override object? GetValue()
        {
            return _result;
        }

    }
}
