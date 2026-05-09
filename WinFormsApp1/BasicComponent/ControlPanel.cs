using GMap.NET.WindowsForms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using TaxiManager.BasicComponent;

namespace TaxiManager.BasicComponent
{
    //侧边栏组件，负责在地图界面左侧显示各种信息和交互组件。
    //它会定期更新显示的内容，并且可以根据需要切换不同的组件。
    public class ControlPanel : Control, IDisposable
    {
        public const int TickInterval = 50;
        public const int SecondTicks = 1000 / TickInterval;

        private readonly GMapControl _gmap;
        public const int ItemPaddingWidth = 25;
        public const int ItemPaddingHeight = 10;
        private readonly Control _parent;
        private readonly Dictionary<string, SideBarItem> _bars = [];
        private readonly GMapOverlay _overlay = new();
        private IComponent? _component;
        public IComponent? CurrentComponent => _component;
        private readonly System.Windows.Forms.Timer _timer;
        private bool _disposedValue;

        public bool IsExpanded { get; private set; }

        public ControlPanel(Control parent, GMapControl gmap)
        {
            this._parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this._gmap = gmap;

            // 使侧栏不与窗口底部信息栏重合
            this.Height = parent.ClientSize.Height - 25;

            this.Dock = DockStyle.Left;
            BringToFront();

            // 一秒20次更新
            _timer = new System.Windows.Forms.Timer { Interval = TickInterval };
            _timer.Tick += Update;
            _timer.Start();

            // adjust height on parent resize
            parent.Resize += OnResize;

            gmap.Overlays.Add(_overlay);
        }

        private void Update(object? sender, EventArgs e)
        {
            bool hasResizeDirty = false;
            foreach (var bar in _bars.Values)
            {
                bar.TickUpdate();
                hasResizeDirty |= bar.ResizeDirty;
            }
            if (hasResizeDirty)
                UpdateLayout();
            _component?.Update(this);
        }

        private void OnResize(object? sender, EventArgs e)
        {
            //减去25像素的底部间距，使侧边栏高度不与窗口底部贴边。
            this.Height = _parent.ClientSize.Height - 25;
        }

        /// <summary>
        /// 获取 SideBarItem 组件储存的值
        /// </summary>
        public object? GetItemValue(string itemKey)
        {
            return _bars[itemKey]?.GetValue();
        }

        public void UpdateLayout()
        {
            int maxWidth = 0;
            int yOffset = ItemPaddingHeight;
            foreach (var bar in _bars.Values)
            {
                bar.ResizeDirty = false;
                maxWidth = Math.Max(maxWidth, bar.Width);
                bar.Location = new(ItemPaddingWidth, yOffset);
                yOffset += bar.Height + ItemPaddingHeight;
            }
            Width = maxWidth <= 0 ? 0 : maxWidth + ItemPaddingWidth * 2;
        }

        public void SwitchTo(IComponent? component)
        {
            foreach (var bar in _bars.Values)
                this.Controls.Remove(bar);
            _bars.Clear();
            _overlay.Clear();
            List<(string, SideBarItem)> registry = [];
            component?.RegisterBars(registry);
            foreach (var entry in registry)
            {
                var bar = entry.Item2;
                this.Controls.Add(bar);
                this._bars.Add(entry.Item1, entry.Item2);
            }
            UpdateLayout();
            _component = component;
            _component?.Update(this);
        }

        public void ModifyOverlay(Action<GMapOverlay> handler)
        {
            _gmap.HoldInvalidation = true;
            _overlay.Clear();
            handler.Invoke(_overlay);
            _gmap.HoldInvalidation = false;
            _gmap.Refresh();
        }

        void IDisposable.Dispose()
        {
            _timer?.Dispose();
            if (_parent != null) _parent.Resize -= OnResize;
            _gmap.Overlays.Remove(_overlay);
            GC.SuppressFinalize(this);
        }
    }
}
