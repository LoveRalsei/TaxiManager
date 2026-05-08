using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TaxiManager.UI
{
    public abstract class SideBarItem : Control
    {
        public const int InnerPaddingHeight = 10;
        public string? _text;
        public SideBarItem(string? text)
        {
            _text = text;
            InitComponents();
        }

        public virtual void AddControlComponent(Control control)
        {
            int totalHeight = 0;
            int maxWidth = control.Width;
            foreach (Control c in Controls)
            {
                totalHeight += c.Height + InnerPaddingHeight;
                maxWidth = Math.Max(maxWidth, c.Width);
            }
            control.Location = new Point(0, totalHeight);
            this.Height = totalHeight + control.Height;
            this.Width = maxWidth;
            Controls.Add(control);
        }

        public virtual void InitComponents()
        {
            if (_text != null)
            {
                AddControlComponent(new Label
                {
                    Text = _text,
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleLeft
                });
            }
        }

        public abstract object? GetValue();
        public virtual void SetValue(object? value) { }

        public virtual void TickUpdate() { }
    }
}
