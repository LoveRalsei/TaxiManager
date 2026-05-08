using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TaxiManager.BasicComponent
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
        
        public virtual void UpdateSize()
        {
            int totalHeight = 0;
            int maxWidth = 0;
            foreach (Control c in Controls)
            {
                totalHeight += c.Height;
                maxWidth = Math.Max(maxWidth, c.Width);
            }
            if (Controls.Count > 0)
                totalHeight += InnerPaddingHeight * (Controls.Count - 1);
            this.Height = totalHeight;
            this.Width = maxWidth;
        }

        public virtual void AddControlComponent(Control control)
        {
            int totalHeight = 0;
            foreach (Control c in Controls)
            {
                totalHeight += c.Height + InnerPaddingHeight;
            }
            control.Location = new Point(0, totalHeight);
            Controls.Add(control);
            UpdateSize();
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
