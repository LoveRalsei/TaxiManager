using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TaxiManager.UI
{
    /// <summary>
    /// 用于功能F3（区域范围查找）的侧边栏，包含日期选择器。
    /// </summary>
    public class SideBarChooseTimePeriod : SideBarItem
    {
        public static readonly DateTime MinTime = DataLoader.TimeMin;
        public static readonly DateTime MaxTime = DataLoader.TimeMax;

        private DateTimePicker? _pickerFrom;
        private DateTimePicker? _pickerTo;

        public SideBarChooseTimePeriod() : base("选择时段") { }

        public override object? GetValue()
        {
            if (_pickerFrom!.Value >= _pickerTo!.Value)
                return null;
            return (_pickerFrom!.Value, _pickerTo!.Value);
        }

        public override void InitComponents()
        {

            // DateTimePicker for year/month/day selection - Start Date
            _pickerFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                Value = MinTime,
                MinDate = MinTime,
                MaxDate = MaxTime,
                Height = 28,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                CalendarFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };

            // DateTimePicker for year/month/day selection - End Date
            _pickerTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                Value = MaxTime,
                MinDate = MinTime,
                MaxDate = MaxTime,
                Height = 28,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                CalendarFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };

            // Apply light-blue style where supported
            _pickerFrom.CalendarForeColor = Color.FromArgb(20, 60, 120);
            _pickerTo.CalendarForeColor = Color.FromArgb(20, 60, 120);
            try
            {
                _pickerFrom.CalendarMonthBackground = Color.FromArgb(240, 248, 255);
                _pickerTo.CalendarMonthBackground = Color.FromArgb(240, 248, 255);
            }
            catch { }

            AddControlComponent(_pickerFrom);
            AddControlComponent( _pickerTo );
        }

    }
}
