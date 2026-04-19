using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TaxiManager
{
    /// <summary>
    /// 用于功能F3（区域范围查找）的侧边栏，包含日期选择器。
    /// </summary>
    public class LeftSidebar_ChooseTimePeriod : LeftSidebar
    {
        private DateTimePicker _datePicker1;
        private DateTimePicker _datePicker2;
        private Panel _contentPanel;
        private Label _errorLabel;

        public LeftSidebar_ChooseTimePeriod()
        {
            InitializeDatePickers();
        }

        private void InitializeDatePickers()
        {
            // Find the content panel (it's the second control after _titleLabel)
            _contentPanel = null;
            foreach (Control c in Controls)
            {
                if (c is Panel panel && c != _titleLabel)
                {
                    _contentPanel = panel;
                    break;
                }
            }

            if (_contentPanel == null) return;

            // Error label for insufficient regions
            _errorLabel = new Label
            {
                Text = "选择区域不足",
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                Height = 20,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 8, 0, 4),
                Visible = false
            };

            // DateTimePicker for year/month/day selection - Start Date
            _datePicker1 = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Value = new DateTime(2008, 2, 2),
                Height = 28,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 12, 0, 12),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                CalendarFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };

            // DateTimePicker for year/month/day selection - End Date
            _datePicker2 = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Value = new DateTime(2008, 2, 2),
                Height = 28,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 12, 0, 12),
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

            // Insert date pickers into the content panel (between spacer and text)
            // Controls are added in reverse order for Dock.Top
            int textIndex = _contentPanel.Controls.IndexOf(_text);
            if (textIndex >= 0)
            {
                _contentPanel.Controls.Add(_datePicker2);
                _contentPanel.Controls.SetChildIndex(_datePicker2, textIndex);
                _contentPanel.Controls.Add(_datePicker1);
                _contentPanel.Controls.SetChildIndex(_datePicker1, textIndex);
                _contentPanel.Controls.Add(_errorLabel);
                _contentPanel.Controls.SetChildIndex(_errorLabel, textIndex);
            }
        }

        [Category("Behavior")]
        public bool ErrorMessageVisible
        {
            get => _errorLabel.Visible;
            set => _errorLabel.Visible = value;
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
    }
}
