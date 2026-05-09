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
        /// <summary>
        /// 标志信息，用于更新 ControlPanel 的大小
        /// </summary>
        public bool ResizeDirty = false;
        public SideBarItem(string? text)
        {
            _text = text;
            InitComponents();
        }
        //根据已有组件的高度和间距来设置当前组件的大小
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
            ResizeDirty = true;
        }
        //根据已有组件的高度和间距来设置新组件的位置，并添加到控件集合中
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
        // 检测并初始化文本组件
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
