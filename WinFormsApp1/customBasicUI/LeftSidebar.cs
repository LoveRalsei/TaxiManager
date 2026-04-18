using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TaxiManager
{
    public class LeftSidebar : UserControl
    {
        private Label _titleLabel;
        private Label _text;
        private DateTimePicker _datePicker1;
        private DateTimePicker _datePicker2;
        private Button _bottomButton;

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
                Text = "111",
                Height = 28,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 4, 0, 8),
                //BackColor = Color.FromArgb(240, 248, 255), // 淡蓝
                ForeColor = Color.FromArgb(20, 60, 120),
                //BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point)
            };

            // DateTimePicker for year/month/day selection
            _datePicker1 = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Value = new DateTime(2008, 2, 2),
                Height = 28,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 4, 0, 8),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                CalendarFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };
            _datePicker2 = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Value = new DateTime(2008, 2, 2),
                Height = 28,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 4, 0, 8),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                CalendarFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };
            // Apply light-blue style where supported
            _datePicker1.CalendarForeColor = Color.FromArgb(20, 60, 120);
            _datePicker2.CalendarForeColor = Color.FromArgb(20, 60, 120);
            try
            {
                _datePicker1.CalendarMonthBackground = Color.FromArgb(240, 248, 255);
                _datePicker2.CalendarMonthBackground = Color.FromArgb(240, 248, 255);
            }
            catch { }

            // Spacer
            var spacer = new Panel { Height = 8, Dock = DockStyle.Top }; 

            // Bottom button
            _bottomButton = new Button
            {
                Text = "确定",
                Height = 36,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(200, 230, 255),
                ForeColor = Color.FromArgb(10, 70, 140),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point)
            };
            _bottomButton.FlatAppearance.BorderSize = 0;

            // Add controls into content panel in top-down order
            content.Controls.Add(_bottomButton);
            content.Controls.Add(spacer);
            content.Controls.Add(_datePicker1);
            content.Controls.Add(_datePicker2);
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
        public DateTime SelectedStartDate
        {
            get => _datePicker1.Value.Date;
            set => _datePicker1.Value = value.Date;
        }

        [Category("Behavior")]
        public DateTime SelectedEndDate
        {
            get => _datePicker2.Value.Date;
            set => _datePicker2.Value = value.Date;
        }

        [Category("Behavior")]
        public string StartDateString => _datePicker1.Value.ToString("yyyy-MM-dd");

        [Category("Behavior")]
        public string EndDateString => _datePicker2.Value.ToString("yyyy-MM-dd");

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
