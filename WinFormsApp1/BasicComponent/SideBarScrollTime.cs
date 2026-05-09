using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxiManager.BasicComponent;
using TaxiManager;

namespace TaxiManager.BasicComponent
{
    //时间滚动条组件
    public class SideBarScrollTime : SideBarItem
    {

        public static readonly DateTime MinTime = DataLoader.TimeMin;
        public static readonly DateTime MaxTime = DataLoader.TimeMax;
        public static readonly TimeSpan MinSpeed = TimeSpan.FromSeconds(3);
        public static readonly TimeSpan MaxSpeed = TimeSpan.FromMinutes(15);
        public TimeSpan PlaySpeed = TimeSpan.FromMinutes(5);
        private DateTime _curr = MinTime;
        
        private DateTimePicker? _picker;
        private Button? _button;
        private TrackBar? _timeTrack;
        private FlowLayoutPanel? _timeTrackBar;
        private Label? _speedLabel;
        private TrackBar? _speedTrack;

        private double _cacheSeconds = 0;

        private bool _isPlaying = false;

        public SideBarScrollTime() : base("时间选择") { }
        public override void InitComponents()
        {
            base.InitComponents();

            // 滚动条更新 -> 更新时间选择器 -> 更新记录时间
            _picker = new DateTimePicker
            {
                Width = 240,
                Height = 28,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                Value = _curr,
                MinDate = MinTime,
                MaxDate = MaxTime
            };
            _picker.ValueChanged += OnPickerChanged;
            AddControlComponent(_picker);

            // 滚动条+按钮组合成一个横条大组件
            _button = new Button
            {
                Width = 35,
                Height = 40,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
            };
            _button.Click += OnButtonClick;
            _isPlaying = false;
            UpdateButtonIcon();

            _timeTrack = new TrackBar
            {
                Width = 200,
                Height = 45,
                Minimum = 0,
                Maximum = (int)((MaxTime - MinTime).TotalSeconds),
                Value = 0,
                Orientation = Orientation.Horizontal,
            };
            _timeTrack.ValueChanged += OnTimeTrackChanged;
            _timeTrack.Scroll += OnTimeTrackUserScroll;
            _timeTrackBar = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Width = 260,
                Height = 50,
                Padding = new Padding(5, 0, 5, 0)
            };
            _timeTrackBar.Controls.Add(_button);
            _timeTrackBar.Controls.Add(_timeTrack);
            AddControlComponent(_timeTrackBar);

            _speedLabel = new Label
            {
                Text = "播放速度 (6000s / s)",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            AddControlComponent(_speedLabel);
            _speedTrack = new TrackBar
            {
                Width = 240,
                Height = 45,
                Minimum = (int)MinSpeed.TotalMilliseconds,
                Maximum = (int)MaxSpeed.TotalMilliseconds,
                Value = (int)PlaySpeed.TotalMilliseconds
            };
            _speedTrack.ValueChanged += OnSpeedTrackChanged;
            AddControlComponent (_speedTrack);
        }
        public virtual void UpdateButtonIcon()
        {
            if (_button == null)
                throw new InvalidOperationException("Update button too early!");
            if (_isPlaying)
                _button.Text = "⏸";
            else
                _button.Text = "▶";
        }
        public virtual void OnButtonClick(object? sender, EventArgs e)
        {
            _isPlaying = !_isPlaying;
            UpdateButtonIcon();
        }
        //用户拖动滚动条时，停止播放
        public virtual void OnTimeTrackUserScroll(object? sender, EventArgs e)
        {
            _isPlaying = false;
            UpdateButtonIcon();
        }
        //滚动条更新时，更新时间选择器的值
        public virtual void OnTimeTrackChanged(object? sender, EventArgs e)
        {
            _curr = MinTime + TimeSpan.FromSeconds(_timeTrack!.Value);
            if (_picker!.Value != _curr)
                _picker!.Value = _curr;
        }
        //播放速度滚动条更新时，修改播放速度并更新标签显示
        public virtual void OnSpeedTrackChanged(object? sender, EventArgs e)
        {
            PlaySpeed = TimeSpan.FromMilliseconds(_speedTrack!.Value);
            _speedLabel!.Text = $"播放速度 ({(int)(PlaySpeed.TotalSeconds * ControlPanel.SecondTicks)}s / s)";
        }
        public virtual void OnPickerChanged(object? sender, EventArgs args)
        {
            _curr = _picker!.Value;
            int seconds = (int)((_curr - MinTime).TotalSeconds);
            if (_timeTrack!.Value != seconds)
                _timeTrack!.Value = seconds;
        }
        public override object? GetValue()
        {
            return _curr;
        }
        //循环更新方法
        public override void TickUpdate()
        {
            base.TickUpdate();
            if (_isPlaying)
            {
                _cacheSeconds += PlaySpeed.TotalSeconds;
                if (_cacheSeconds >= 1)
                    _timeTrack!.Value = (int)Math.Min(_timeTrack.Maximum, (_curr - MinTime).TotalSeconds + _cacheSeconds);
                _cacheSeconds %= 1;
            }
        }
    }
}
