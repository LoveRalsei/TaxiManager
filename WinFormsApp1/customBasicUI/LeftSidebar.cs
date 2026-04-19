using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TaxiManager
{
    public class LeftSidebar : UserControl
    {
        protected Label _titleLabel;
        protected Label _text;
        protected Button _bottomButton;

        public LeftSidebar()
        {
            InitializeComponents();
        }

        public void InitializeComponents()
        {

            BackColor = Color.White;
            Width = 260;
            // We'll not dock by default; SidebarController will place it.

            _titleLabel = new Label
            {
                Text = "侧边栏",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(30, 90, 160),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 36,
                Dock = DockStyle.Top,
                Padding = new Padding(12, 0, 0, 0)
            };

            // content panel with padding
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 12, 12, 12),
                BackColor = Color.White
            };

            // TextBox as the text control
            _text = new Label
            {
                Text = "请选择起始时间与结束时间",
                Height = 28,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 4, 0, 8),
                //BackColor = Color.FromArgb(240, 248, 255), // 淡蓝
                ForeColor = Color.FromArgb(20, 60, 120),
                //BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point)
            };

            // Spacer
            var spacer = new Panel { Height = 8, Dock = DockStyle.Top };

            // Bottom button (with margin to avoid overlapping with status strip)
            var bottomButtonPanel = new Panel
            {
                Height = 44,
                Dock = DockStyle.Bottom,
                BackColor = Color.White
            };

            _bottomButton = new Button
            {
                Text = "确定",
                Height = 36,
                Width = 236,
                BackColor = Color.FromArgb(200, 230, 255),
                ForeColor = Color.FromArgb(10, 70, 140),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(12, 4)
            };
            _bottomButton.FlatAppearance.BorderSize = 0;
            bottomButtonPanel.Controls.Add(_bottomButton);

            // Add controls into content panel in top-down order
            content.Controls.Add(bottomButtonPanel);
            content.Controls.Add(spacer);
            content.Controls.Add(_text);

            Controls.Add(content);
            Controls.Add(_titleLabel);

            // Initial state: enabled and visible (controller will manage)
            SetExpanded(false);
        }

        [Category("Appearance")]
        public string Title
        {
            get => _titleLabel.Text;
            set => _titleLabel.Text = value;
        }

        [Category("Behavior")]
        public string TextValue
        {
            get => _text.Text;
            set => _text.Text = value;
        }

        [Category("Appearance")]
        public Button BottomButton => _bottomButton;

        /// <summary>
        /// 设置侧边栏展开或收起时子控件的可见性与启用状态。
        /// 收起时子控件隐藏并 Disabled，展开时显示并 Enabled。
        /// </summary>
        public void SetExpanded(bool expanded)
        {
            // Title stays visible when collapsed for small header (optional)
            // But per requirement, when collapsed child controls should be hidden and disabled.
            foreach (Control c in Controls)
            {
                if (c == _titleLabel) continue;
                c.Visible = expanded;
                c.Enabled = expanded;
            }
            // Ensure title is always visible
            _titleLabel.Visible = true;
            _titleLabel.Enabled = true;
        }
    }
}
