using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiManager
{
    public partial class MapForm : Form
    {
        private bool _isRegionSearching = false;
        private bool _isRegionDragging = false;
        private List<PointLatLng> _regionSearchPoints = new List<PointLatLng>();
        private GMapOverlay _regionSearchOverlay = new GMapOverlay("polygons");
        private Point _regionDragStartLocal;
        private Point _regionDragCurrentLocal;

        // 替换或使用的按钮切换函数（调用即可）
        private void _regionSreachButtonClick(object sender, EventArgs e)
        {
            if (_isRegionSearching == false)
            {
                // 启用区域选择：订阅鼠标事件，清理缓存
                _isRegionSearching = true;
                _regionSearchPoints.Clear();
                _regionSearchOverlay.Polygons.Clear();
                _regionSearchOverlay.Markers.Clear();

                gmap.MouseDown += _mapRegionMouseDown;
                gmap.MouseMove += _mapRegionMouseMove;
                gmap.MouseUp += _mapRegionMouseUp;

                if (!gmap.Overlays.Contains(_regionSearchOverlay))
                    gmap.Overlays.Add(_regionSearchOverlay);

                _regionSreachButton.Text = "区域选择中...";
            }
            else
            {
                // 停用区域选择：取消订阅
                _isRegionSearching = false;

                _regionSearchOverlay.Polygons.Clear();
                _regionSearchOverlay.Markers.Clear();

                gmap.MouseDown -= _mapRegionMouseDown;
                gmap.MouseMove -= _mapRegionMouseMove;
                gmap.MouseUp -= _mapRegionMouseUp;
                _regionSreachButton.Text = "开始区域选择";

            }
            // 同时切换侧边栏显示/隐藏（如果已初始化）
            try
            {
                sidebarController?.Toggle();
            }
            catch { }
        }

        // 鼠标按下：开始拖拽
        private void _mapRegionMouseDown(object sender, MouseEventArgs e)
        {
            if (_isRegionSearching == false) return;
            if (e.Button != MouseButtons.Left) return;

            _isRegionDragging = true;
            _regionDragStartLocal = new Point(e.X, e.Y);
            _regionDragCurrentLocal = _regionDragStartLocal;

            // 清理上一次绘制
            _regionSearchOverlay.Polygons.Clear();
            _regionSearchOverlay.Markers.Clear();
        }

        // 鼠标移动：更新临时矩形显示
        private void _mapRegionMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isRegionSearching || !_isRegionDragging) return;

            _regionDragCurrentLocal = new Point(e.X, e.Y);

            // 计算矩形四个角（控件坐标）
            int left = Math.Min(_regionDragStartLocal.X, _regionDragCurrentLocal.X);
            int right = Math.Max(_regionDragStartLocal.X, _regionDragCurrentLocal.X);
            int top = Math.Min(_regionDragStartLocal.Y, _regionDragCurrentLocal.Y);
            int bottom = Math.Max(_regionDragStartLocal.Y, _regionDragCurrentLocal.Y);

            var cornersLocal = new Point[]
            {
        new Point(left, top),
        new Point(right, top),
        new Point(right, bottom),
        new Point(left, bottom)
            };

            // 把控件坐标转为经纬度
            var polyPoints = new List<PointLatLng>();
            foreach (var p in cornersLocal)
                polyPoints.Add(gmap.FromLocalToLatLng(p.X, p.Y));

            // 重建 overlay（清除旧的临时图形）
            _regionSearchOverlay.Polygons.Clear();
            _regionSearchOverlay.Markers.Clear();

            // 多边形（半透明填充）
            var polygon = new GMapPolygon(polyPoints, "选择矩形");
            polygon.Fill = new SolidBrush(Color.FromArgb(20, Color.Blue));
            polygon.Stroke = new Pen(Color.Blue, 2);
            _regionSearchOverlay.Polygons.Add(polygon);

            // 在四个角添加标记（便于识别）
            for (int i = 0; i < polyPoints.Count; i++)
            {
                var marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(polyPoints[i],
                    GMap.NET.WindowsForms.Markers.GMarkerGoogleType.red_small);
                // 可在 marker.Tag 存序号或其它信息
                _regionSearchOverlay.Markers.Add(marker);
            }

            // 确保 overlay 已经在 gmap 上
            if (!gmap.Overlays.Contains(_regionSearchOverlay))
                gmap.Overlays.Add(_regionSearchOverlay);
        }

        // 鼠标抬起：结束拖拽，固定图形并调用分析逻辑
        private void _mapRegionMouseUp(object sender, MouseEventArgs e)
        {
            if (!_isRegionSearching || !_isRegionDragging) return;
            if (e.Button != MouseButtons.Left)
            {
                // 右键或其它键可以取消当前选择
                _isRegionDragging = false;
                return;
            }

            _isRegionDragging = false;

            // 计算并保存最终四角经纬度（与 MouseMove 中相同）
            int left = Math.Min(_regionDragStartLocal.X, _regionDragCurrentLocal.X);
            int right = Math.Max(_regionDragStartLocal.X, _regionDragCurrentLocal.X);
            int top = Math.Min(_regionDragStartLocal.Y, _regionDragCurrentLocal.Y);
            int bottom = Math.Max(_regionDragStartLocal.Y, _regionDragCurrentLocal.Y);

            var cornersLocal = new Point[]
            {
        new Point(left, top),
        new Point(right, top),
        new Point(right, bottom),
        new Point(left, bottom)
            };

            _regionSearchPoints.Clear();
            foreach (var p in cornersLocal)
                _regionSearchPoints.Add(gmap.FromLocalToLatLng(p.X, p.Y));

            // 最终图形已在 overlay 中（如果需要可做额外样式调整）
            _regionSreachButton.Text = $"已选区域({_regionSearchPoints.Count})";

            // 调用分析函数由侧边栏底部按钮触发（在此处不直接调用）
        }

        // 区域分析占位函数（你在这里实现具体分析）
        // 新增两个字符串参数：开始时间与结束时间，来自侧边栏的两个 DateTimePicker
        private void _analyzeRegion(List<PointLatLng> polygonCorners, string startTime, string endTime)
        {
            // TODO: 在这里用 polygonCorners（四个角点经纬度）和时间范围做后续处理或过滤数据
            // 示例：polygonCorners[0].Lat / .Lng 可直接使用
            // 示例时间格式： startTime / endTime 为 "yyyy-MM-dd"
        }
    }
}
