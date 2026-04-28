using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TaxiManager
{
    public class SidebarController : IDisposable
    {
        private readonly Control _parent;
        private readonly LeftSidebar _sidebar;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly int _expandedWidth;
        private int _animationStep = 20; // pixels per tick
        private bool _expanding;
        private bool _disposedValue;

        public bool IsExpanded { get; private set; }

        public SidebarController(Control parent, LeftSidebar sidebar, int expandedWidth = 260, int animationIntervalMs = 15)
        {
            this._parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this._sidebar = sidebar ?? throw new ArgumentNullException(nameof(sidebar));
            this._expandedWidth = Math.Max(80, expandedWidth);

            // place sidebar on the left: dock it so layout with other docked controls works
            sidebar.Width = 0;
            sidebar.Height = parent.ClientSize.Height;
            sidebar.Dock = DockStyle.Left;
            sidebar.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            sidebar.Visible = true;
            parent.Controls.Add(sidebar);
            // ensure sidebar is on top of other controls so it is visible when expanded
            sidebar.BringToFront();

            _timer = new System.Windows.Forms.Timer { Interval = animationIntervalMs };
            _timer.Tick += _timerTick;

            // adjust height on parent resize
            parent.Resize += _parentResize;
        }

        private void _parentResize(object? sender, EventArgs e)
        {
            _sidebar.Height = _parent.ClientSize.Height;
        }

        private void _timerTick(object? sender, EventArgs e)
        {
            if (_expanding)
            {
                var w = _sidebar.Width + _animationStep;
                if (w >= _expandedWidth)
                {
                    _sidebar.Width = _expandedWidth;
                    _timer.Stop();
                    IsExpanded = true;
                    _sidebar.SetExpanded(true);
                }
                else
                {
                    _sidebar.Width = w;
                }
            }
            else
            {
                var w = _sidebar.Width - _animationStep;
                if (w <= 0)
                {
                    _sidebar.Width = 0;
                    _timer.Stop();
                    IsExpanded = false;
                    _sidebar.SetExpanded(false);
                }
                else
                {
                    _sidebar.Width = w;
                }
            }
        }

        public void Show()
        {
            Debug.WriteLine("Try Show");
            //if (IsExpanded) return;
            _expanding = true;
            _sidebar.Visible = true;
            _sidebar.BringToFront();
            if (_animationStep >= _expandedWidth)
            {
                _sidebar.Width = _expandedWidth;
                IsExpanded = true;
                _sidebar.SetExpanded(true);
                return;
            }
            _timer.Start();//Debug.WriteLine("Show");
        }

        public void Hide()
        {
            //Debug.WriteLine("Try Hide");
            if (IsExpanded == false) return;
            _expanding = false;
            _sidebar.SetExpanded(false);
            _timer.Start();//Debug.WriteLine("Hide");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _timer?.Dispose();
                    if (_parent != null) _parent.Resize -= _parentResize;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
