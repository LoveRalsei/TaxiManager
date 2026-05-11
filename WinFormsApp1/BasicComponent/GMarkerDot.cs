using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace TaxiManager.BasicComponent
{
    public class GMarkerDot : GMapMarker
    {
        /// <summary>
        /// 圆的半径（单位：像素）
        /// </summary>
        public int Radius { get; set; }

        /// <summary>
        /// 圆形的填充画刷
        /// </summary>
        public Brush FillBrush { get; set; }

        /// <summary>
        /// 圆形的边框画笔
        /// </summary>
        public Pen StrokePen { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="p">点的经纬度坐标</param>
        /// <param name="radius">半径（像素）</param>
        /// <param name="color">圆点颜色</param>
        public GMarkerDot(PointLatLng p, int radius, Color color)
            : base(p)
        {
            this.Radius = radius;
            this.Size = new Size(radius * 2, radius * 2); // 设置标记的大小
            // 设置此标记可以被点击命中测试
            this.IsHitTestVisible = true;
            // 初始化默认的画刷和画笔
            this.FillBrush = new SolidBrush(color);
            this.StrokePen = new Pen(Color.Black, 1); // 默认黑色边框，你也可以设为透明或与填充色相同
        }

        /// <summary>
        /// 重写OnRender方法，用GDI+绘制圆形
        /// </summary>
        public override void OnRender(Graphics g)
        {
            // LocalPosition 是标记在当前缩放的地图上的像素坐标
            int x = LocalPosition.X - Radius;
            int y = LocalPosition.Y - Radius;
            g.FillEllipse(FillBrush, x, y, Size.Width, Size.Height);

            // 如果你想添加边框，可以取消下面这行的注释
            // g.DrawEllipse(StrokePen, x, y, Size.Width, Size.Height);
        }

        /// <summary>
        /// 重写Dispose方法，释放画刷和画笔等资源
        /// </summary>
        public override void Dispose()
        {
            if (FillBrush != null)
            {
                FillBrush.Dispose();
                FillBrush = null;
            }
            if (StrokePen != null)
            {
                StrokePen.Dispose();
                StrokePen = null;
            }
            base.Dispose();
        }
    }
}
