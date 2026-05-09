using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager.BasicComponent
{
    internal class SideBarChooseTime : SideBarItem
    {
        public static readonly DateTime MinTime = DataLoader.TimeMin;
        public static readonly DateTime MaxTime = DataLoader.TimeMax;

        private DateTimePicker? _picker;

        public SideBarChooseTime() : base("选择时间点")
        {
        }

        public override object? GetValue()
        {
            return _picker!.Value;
        }

        public override void InitComponents()
        {
            _picker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                Value = MinTime,
                MinDate = MinTime,
                MaxDate = MaxTime,
                Height = 28,
                Width = 320,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                CalendarFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };
            _picker.CalendarForeColor = Color.FromArgb(20, 60, 120);
            try
            {
                _picker.CalendarMonthBackground = Color.FromArgb(240, 248, 255);
            }
            catch { }
            AddControlComponent(_picker);
        }
    }
}
